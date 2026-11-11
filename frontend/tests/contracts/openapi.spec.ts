import { expect, test } from '../../web/node_modules/@playwright/test';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

type JsonObject = Record<string, unknown>;

const openApi = JSON.parse(
  readFileSync(resolve(process.cwd(), '../../specs/001-garagem-pessoal/contracts/openapi.json'), 'utf8'),
) as JsonObject;
const resources = JSON.parse(
  readFileSync(resolve(process.cwd(), '../../specs/001-garagem-pessoal/contracts/resources.schema.json'), 'utf8'),
) as JsonObject;

const expectedCreationKinds: Record<string, string> = {
  createGarage: 'garage',
  createInvite: 'invite',
  createVehicle: 'vehicle',
  createExpense: 'expense',
  createFuelRecord: 'fuel',
  createElectricCharge: 'electric-charge',
  createElectricityTariff: 'electricity-tariff',
  createMaintenance: 'maintenance',
  createPlan: 'plan',
  createCommitment: 'commitment',
  createAttachment: 'attachment',
  createExport: 'export',
};

function valueAtPointer(document: JsonObject, pointer: string): unknown {
  return pointer
    .slice(2)
    .split('/')
    .reduce<unknown>((current, segment) => {
      const key = segment.replaceAll('~1', '/').replaceAll('~0', '~');
      return (current as JsonObject)[key];
    }, document);
}

function resolveReference(reference: string, document: JsonObject = openApi): unknown {
  if (reference.startsWith('#/')) {
    return valueAtPointer(document, reference);
  }

  const [fileName, pointer] = reference.split('#');
  if (fileName !== 'resources.schema.json' || !pointer?.startsWith('/')) {
    throw new Error(`Unsupported or unresolved contract reference: ${reference}`);
  }

  return valueAtPointer(resources, `#${pointer}`);
}

function resolveSchema(reference: string): JsonObject {
  let resolved = resolveReference(reference);
  const visited = new Set<string>();

  while (resolved !== null && typeof resolved === 'object' && typeof (resolved as JsonObject).$ref === 'string') {
    const nextReference = (resolved as JsonObject).$ref as string;
    if (visited.has(nextReference)) {
      throw new Error(`Cyclic schema reference: ${nextReference}`);
    }
    visited.add(nextReference);
    resolved = resolveReference(nextReference);
  }

  return resolved as JsonObject;
}

function collectReferences(value: unknown, references: string[] = []): string[] {
  if (Array.isArray(value)) {
    value.forEach((item) => collectReferences(item, references));
    return references;
  }

  if (value !== null && typeof value === 'object') {
    const object = value as JsonObject;
    if (typeof object.$ref === 'string') {
      references.push(object.$ref);
    }
    Object.values(object).forEach((item) => collectReferences(item, references));
  }

  return references;
}

function operationAt(operationId: string): { method: string; path: string; operation: JsonObject } {
  for (const [path, pathItem] of Object.entries(openApi.paths as JsonObject)) {
    for (const [method, operation] of Object.entries(pathItem as JsonObject)) {
      if (
        operation !== null &&
        typeof operation === 'object' &&
        (operation as JsonObject).operationId === operationId
      ) {
        return { method, path, operation: operation as JsonObject };
      }
    }
  }

  throw new Error(`Operation is missing from OpenAPI: ${operationId}`);
}

function resolveParameter(parameter: unknown): JsonObject {
  if (parameter !== null && typeof parameter === 'object' && '$ref' in parameter) {
    return resolveReference((parameter as JsonObject).$ref as string) as JsonObject;
  }
  return parameter as JsonObject;
}

test('all OpenAPI references resolve, including the external resource schema', () => {
  const references = collectReferences(openApi);
  expect(references).toContain('resources.schema.json#/$defs/Resource');

  for (const reference of references) {
    expect(resolveReference(reference), reference).toBeDefined();
  }

  for (const reference of collectReferences(resources)) {
    expect(resolveReference(reference, resources), reference).toBeDefined();
  }
});

test('all twelve domain creation operations require an idempotency key and map to a resource kind', () => {
  const definitions = (resources.$defs as JsonObject);
  const resourceUnion = definitions.Resource as JsonObject;
  const variants = resourceUnion.oneOf as JsonObject[];
  const resourceKinds = variants.map((variant) => {
    const resource = resolveReference(variant.$ref as string, resources) as JsonObject;
    return (resource.properties as JsonObject).kind as JsonObject;
  });

  expect(Object.keys(expectedCreationKinds)).toHaveLength(12);

  for (const [operationId, expectedKind] of Object.entries(expectedCreationKinds)) {
    const { method, operation } = operationAt(operationId);
    expect(method).toBe('post');

    const parameters = (operation.parameters as unknown[] | undefined) ?? [];
    const idempotencyKey = parameters.map(resolveParameter).find((parameter) => parameter.name === 'Idempotency-Key');
    expect(idempotencyKey, operationId).toBeDefined();
    expect(idempotencyKey?.required, operationId).toBe(true);
    expect(resourceKinds.some((kind) => kind.const === expectedKind), operationId).toBe(true);

    const successResponse = Object.entries(operation.responses as JsonObject).find(([status]) => status.startsWith('2'));
    expect(successResponse, operationId).toBeDefined();
    const response = resolveReference((successResponse?.[1] as JsonObject).$ref as string) as JsonObject;
    const schema = resolveSchema(
      (((response.content as JsonObject)['application/json'] as JsonObject).schema as JsonObject).$ref as string,
    );
    expect(schema.oneOf, operationId).toBeDefined();
  }
});

test('public resources are closed and member identity maps to userId', () => {
  const definitions = resources.$defs as JsonObject;
  const resourceNames = Object.keys(definitions).filter((name) => name.endsWith('Resource') && name !== 'Resource');
  const attributeNames = Object.keys(definitions).filter((name) => name.endsWith('Attributes'));

  expect(resourceNames).toHaveLength(14);
  expect(attributeNames).toHaveLength(14);

  for (const name of [...resourceNames, ...attributeNames]) {
    expect((definitions[name] as JsonObject).additionalProperties, name).toBe(false);
  }

  const member = definitions.MemberResource as JsonObject;
  const memberProperties = member.properties as JsonObject;
  expect(memberProperties.id).toEqual({ $ref: '#/$defs/Uuid' });
  expect(((memberProperties.attributes as JsonObject).$ref as string)).toBe('#/$defs/MemberAttributes');
  expect(operationAt('listMembers').path).toBe('/garages/{garageId}/members');
  expect(operationAt('changeMemberRole').path).toBe('/garages/{garageId}/members/{id}');

  const memberAttributes = definitions.MemberAttributes as JsonObject;
  expect((memberAttributes.properties as JsonObject).userId).toEqual({ $ref: '#/$defs/Uuid' });
});

test('audit search is owner-only and excludes changed values and private data', () => {
  const operation = operationAt('searchGarageAudit').operation;
  const response = resolveReference(((operation.responses as JsonObject)['200'] as JsonObject).$ref as string) as JsonObject;
  const schema = (response.content as JsonObject)['application/json'] as JsonObject;
  const items = ((schema.schema as JsonObject).properties as JsonObject).items as JsonObject;
  const auditItem = items.items as JsonObject;
  const audit = resolveSchema(auditItem.$ref as string);
  const expectedProperties = ['id', 'occurredUtc', 'action', 'actorId', 'targetId', 'changedFields'];

  expect((operation.responses as JsonObject)['200']).toBeDefined();
  expect((response.description as string).toLowerCase()).toContain('owner ativo');
  expect(audit.additionalProperties).toBe(false);
  expect(Object.keys(audit.properties as JsonObject).sort()).toEqual(expectedProperties.sort());
  expect(JSON.stringify(audit).toLowerCase()).not.toContain('password');
  expect(JSON.stringify(audit).toLowerCase()).not.toContain('token');
});
