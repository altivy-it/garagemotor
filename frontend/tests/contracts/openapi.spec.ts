import { expect, test } from '../../web/node_modules/@playwright/test';
import Ajv2020 from '../../web/node_modules/ajv/dist/2020';
import addFormats from '../../web/node_modules/ajv-formats';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

type JsonObject = Record<string, unknown>;

const openApi = JSON.parse(
  readFileSync(
    resolve(process.cwd(), '../../specs/001-garagem-pessoal/contracts/openapi.json'),
    'utf8',
  ),
) as JsonObject;
const resources = JSON.parse(
  readFileSync(
    resolve(process.cwd(), '../../specs/001-garagem-pessoal/contracts/resources.schema.json'),
    'utf8',
  ),
) as JsonObject;

const validator = new Ajv2020({ strict: false });
addFormats(validator);
validator.addSchema(resources, 'resources.schema.json');
validator.addSchema(openApi, 'openapi.json');

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

const expectedItemKinds: Record<string, string> = {
  ...expectedCreationKinds,
  getGarage: 'garage',
  updateGarage: 'garage',
  transferOwnership: 'garage',
  changeMemberRole: 'member',
  acceptInvite: 'invite',
  getVehicle: 'vehicle',
  updateVehicle: 'vehicle',
  updateExpense: 'expense',
  voidExpense: 'expense',
  updateFuelRecord: 'fuel',
  voidFuelRecord: 'fuel',
  getElectricCharge: 'electric-charge',
  updateElectricCharge: 'electric-charge',
  voidElectricCharge: 'electric-charge',
  updateMaintenance: 'maintenance',
  updatePlan: 'plan',
  updateCommitment: 'commitment',
  completeCommitment: 'commitment',
  getExport: 'export',
};

const expectedCollectionKinds: Record<string, string> = {
  listGarages: 'garage',
  listMembers: 'member',
  listVehicles: 'vehicle',
  listExpenses: 'expense',
  listFuelRecords: 'fuel',
  listElectricCharges: 'electric-charge',
  listElectricityTariffs: 'electricity-tariff',
  listMaintenance: 'maintenance',
  listPlans: 'plan',
  listCommitments: 'commitment',
  listReminders: 'reminder',
};

const nullableAttributes: Record<string, string[]> = {
  VehicleAttributes: ['plate', 'renavam', 'chassis'],
  ExpenseAttributes: ['originId'],
  FuelAttributes: ['station', 'historicalReason'],
  ElectricChargeAttributes: ['station', 'tariffRateMinorPerKwh'],
  MaintenanceAttributes: ['amountMinor', 'planId', 'expenseId', 'expenseDate'],
  PlanAttributes: ['dueOn', 'dueKm', 'intervalMonths', 'intervalKm'],
  CommitmentAttributes: [
    'dueOn',
    'dueKm',
    'intervalMonths',
    'intervalKm',
    'completedOn',
    'completedKm',
  ],
  ReminderAttributes: ['dueOn', 'dueKm', 'lastReadingOn', 'lastReadingKm'],
  AttachmentAttributes: ['parentDate', 'sha256'],
  ExportAttributes: ['sha256'],
};

const uuid = '11111111-1111-4111-8111-111111111111';
const garage = {
  id: uuid,
  version: 1,
  kind: 'garage',
  attributes: {
    name: 'Garagem de teste',
    timezone: 'America/Sao_Paulo',
    state: 'active',
    ownerUserId: uuid,
  },
};
const vehicle = {
  id: uuid,
  version: 1,
  kind: 'vehicle',
  attributes: {
    label: 'Veículo de teste',
    kind: 'car',
    brand: 'Marca',
    model: 'Modelo',
    year: 2020,
    fuel: 'electric',
    plate: null,
    renavam: null,
    chassis: null,
    state: 'active',
  },
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

  while (
    resolved !== null &&
    typeof resolved === 'object' &&
    typeof (resolved as JsonObject).$ref === 'string'
  ) {
    const nextReference = (resolved as JsonObject).$ref as string;
    if (visited.has(nextReference)) {
      throw new Error(`Cyclic schema reference: ${nextReference}`);
    }
    visited.add(nextReference);
    resolved = resolveReference(nextReference);
  }

  return resolved as JsonObject;
}

function allowedResourceKinds(
  schema: JsonObject,
  document: JsonObject = openApi,
  visited: Set<string> = new Set(),
): string[] {
  if (typeof schema.$ref === 'string') {
    const key = `${document === openApi ? 'openapi' : 'resources'}:${schema.$ref}`;
    if (visited.has(key)) {
      throw new Error(`Cyclic resource schema reference: ${schema.$ref}`);
    }
    visited.add(key);
    const referencedDocument = schema.$ref.startsWith('resources.schema.json#')
      ? resources
      : document;
    const resolved = resolveReference(schema.$ref, document);
    if (resolved === null || typeof resolved !== 'object' || Array.isArray(resolved)) {
      throw new Error(`Invalid resource schema reference: ${schema.$ref}`);
    }
    const kinds = allowedResourceKinds(resolved as JsonObject, referencedDocument, visited);
    visited.delete(key);
    return kinds;
  }

  if (Array.isArray(schema.oneOf)) {
    return (schema.oneOf as JsonObject[]).flatMap((variant) =>
      allowedResourceKinds(variant, document, visited),
    );
  }

  const properties = schema.properties as JsonObject | undefined;
  const kind = properties?.kind as JsonObject | undefined;
  if (typeof kind?.const !== 'string') {
    throw new Error('Resource schema does not declare a constant kind.');
  }
  return [kind.const];
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

function successResponse(operationId: string): {
  response: JsonObject;
  schema: JsonObject;
  schemaPointer: string;
} {
  const { method, path, operation } = operationAt(operationId);
  const success = Object.entries(operation.responses as JsonObject).find(([status]) =>
    status.startsWith('2'),
  );
  if (!success) {
    throw new Error(`Successful response is missing: ${operationId}`);
  }

  const [status, responseValue] = success;
  const responseReference = responseValue as JsonObject;
  const pointer =
    typeof responseReference.$ref === 'string'
      ? responseReference.$ref
      : `#/paths/${path.replaceAll('~', '~0').replaceAll('/', '~1')}/${method}/responses/${status}`;
  const response =
    typeof responseReference.$ref === 'string'
      ? (resolveReference(responseReference.$ref) as JsonObject)
      : responseReference;
  const schema = ((response.content as JsonObject)['application/json'] as JsonObject)
    .schema as JsonObject;
  return {
    response,
    schema,
    schemaPointer: `openapi.json${pointer}/content/application~1json/schema`,
  };
}

function collectionItemSchema(schema: JsonObject): JsonObject {
  const collection = typeof schema.$ref === 'string' ? resolveSchema(schema.$ref) : schema;
  const items = (collection.properties as JsonObject).items as JsonObject;
  return items.items as JsonObject;
}

function responseValidator(operationId: string) {
  const { schemaPointer } = successResponse(operationId);
  const validate = validator.getSchema(schemaPointer);
  if (!validate) {
    throw new Error(`Response schema could not be compiled: ${operationId}`);
  }
  return validate;
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

test('all twelve domain creation operations require an idempotency key', () => {
  expect(Object.keys(expectedCreationKinds)).toHaveLength(12);

  for (const operationId of Object.keys(expectedCreationKinds)) {
    const { method, operation } = operationAt(operationId);
    expect(method).toBe('post');

    const parameters = (operation.parameters as unknown[] | undefined) ?? [];
    const idempotencyKey = parameters
      .map(resolveParameter)
      .find((parameter) => parameter.name === 'Idempotency-Key');
    expect(idempotencyKey, operationId).toBeDefined();
    expect(idempotencyKey?.required, operationId).toBe(true);
  }
});

test('all twelve domain creation and other operations declare only their own resource kind', () => {
  for (const [operationId, expectedKind] of Object.entries(expectedItemKinds)) {
    const { response, schema } = successResponse(operationId);
    expect.soft(allowedResourceKinds(schema), operationId).toEqual([expectedKind]);
    expect
      .soft((response.headers as JsonObject).ETag, `${operationId} ETag`)
      .toEqual({ schema: { type: 'string' } });
  }

  for (const [operationId, expectedKind] of Object.entries(expectedCollectionKinds)) {
    const item = collectionItemSchema(successResponse(operationId).schema);
    expect.soft(allowedResourceKinds(item), operationId).toEqual([expectedKind]);
  }
});

test('search retains the resource union while domain responses reject other kinds and private fields', () => {
  const garageResponse = responseValidator('createGarage');
  const vehicleList = responseValidator('listVehicles');
  const search = responseValidator('searchGarageRecords');
  const searchKinds = allowedResourceKinds(
    collectionItemSchema(successResponse('searchGarageRecords').schema),
  );
  const expectedSearchKinds = [
    ...new Set([...Object.values(expectedItemKinds), ...Object.values(expectedCollectionKinds)]),
  ];

  expect(searchKinds.sort()).toEqual(expectedSearchKinds.sort());
  expect(garageResponse(garage)).toBe(true);
  expect(garageResponse(vehicle)).toBe(false);
  expect(garageResponse({ ...garage, unexpected: true })).toBe(false);
  expect(garageResponse({ ...garage, version: '1' })).toBe(false);
  expect(
    garageResponse({ ...garage, attributes: { ...garage.attributes, passwordHash: 'fictício' } }),
  ).toBe(false);
  expect(vehicleList({ items: [vehicle], nextCursor: null })).toBe(true);
  expect(vehicleList({ items: [vehicle], nextCursor: 'cursor' })).toBe(true);
  expect(vehicleList({ items: [vehicle] })).toBe(false);
  expect(vehicleList({ items: [vehicle], nextCursor: 123 })).toBe(false);
  expect(vehicleList({ items: [garage], nextCursor: null })).toBe(false);
  expect(search({ items: [garage, vehicle], nextCursor: null })).toBe(true);
});

test('public resources are closed and member identifiers use UUID schemas', () => {
  const definitions = resources.$defs as JsonObject;
  const resourceNames = Object.keys(definitions).filter(
    (name) => name.endsWith('Resource') && name !== 'Resource',
  );
  const attributeNames = Object.keys(definitions).filter((name) => name.endsWith('Attributes'));

  expect(resourceNames).toHaveLength(14);
  expect(attributeNames).toHaveLength(14);

  for (const name of [...resourceNames, ...attributeNames]) {
    expect((definitions[name] as JsonObject).additionalProperties, name).toBe(false);
  }

  const member = definitions.MemberResource as JsonObject;
  const memberProperties = member.properties as JsonObject;
  expect(memberProperties.id).toEqual({ $ref: '#/$defs/Uuid' });
  expect((memberProperties.attributes as JsonObject).$ref as string).toBe(
    '#/$defs/MemberAttributes',
  );
  expect(operationAt('listMembers').path).toBe('/garages/{garageId}/members');
  expect(operationAt('changeMemberRole').path).toBe('/garages/{garageId}/members/{id}');

  const memberAttributes = definitions.MemberAttributes as JsonObject;
  expect((memberAttributes.properties as JsonObject).userId).toEqual({ $ref: '#/$defs/Uuid' });
});

test('nullable response attributes remain required and reject incompatible types', () => {
  const definitions = resources.$defs as JsonObject;
  const attributeNames = Object.keys(definitions).filter((name) => name.endsWith('Attributes'));

  for (const name of attributeNames) {
    const attributes = definitions[name] as JsonObject;
    const properties = attributes.properties as JsonObject;
    const actualNullable = Object.entries(properties)
      .filter(([, property]) => {
        const schema = property as JsonObject;
        const resolved =
          typeof schema.$ref === 'string'
            ? (resolveReference(schema.$ref, resources) as JsonObject)
            : schema;
        return Array.isArray(resolved.type) && resolved.type.includes('null');
      })
      .map(([property]) => property)
      .sort();
    expect(actualNullable, name).toEqual([...(nullableAttributes[name] ?? [])].sort());
    for (const property of actualNullable) {
      expect(attributes.required, `${name}.${property}`).toContain(property);
    }
  }

  const validate = responseValidator('getVehicle');
  expect(validate(vehicle)).toBe(true);
  expect(validate({ ...vehicle, attributes: { ...vehicle.attributes, plate: 123 } })).toBe(false);
  const { plate, ...withoutPlate } = vehicle.attributes;
  expect(plate).toBeNull();
  expect(validate({ ...vehicle, attributes: withoutPlate })).toBe(false);
});

test('electric charge inputs discriminate home and public costs', () => {
  const validate = validator.getSchema('openapi.json#/components/schemas/ElectricChargeInput');
  if (!validate) {
    throw new Error('Electric charge input schema could not be compiled.');
  }

  const base = {
    vehicleId: uuid,
    occurredOn: '2026-11-13',
    energyWh: 1000,
    odometerKm: 100,
    fullAtEnd: true,
  };
  expect(validate({ ...base, source: 'home', station: null })).toBe(true);
  expect(validate({ ...base, source: 'public', amountMinor: 1 })).toBe(true);
  expect(validate({ ...base, source: 'home', amountMinor: 1 })).toBe(false);
  expect(validate({ ...base, source: 'public' })).toBe(false);
  expect(validate({ ...base, source: 'public', amountMinor: 0 })).toBe(false);
  expect(validate({ ...base, source: 'other' })).toBe(false);
});

test('audit search is owner-only and excludes changed values and private data', () => {
  const operation = operationAt('searchGarageAudit').operation;
  const response = resolveReference(
    ((operation.responses as JsonObject)['200'] as JsonObject).$ref as string,
  ) as JsonObject;
  const schema = (response.content as JsonObject)['application/json'] as JsonObject;
  const items = ((schema.schema as JsonObject).properties as JsonObject).items as JsonObject;
  const auditItem = items.items as JsonObject;
  const audit = resolveSchema(auditItem.$ref as string);
  const expectedProperties = [
    'id',
    'occurredUtc',
    'action',
    'actorId',
    'targetId',
    'changedFields',
  ];

  expect((operation.responses as JsonObject)['200']).toBeDefined();
  expect((response.description as string).toLowerCase()).toContain('owner ativo');
  expect(audit.additionalProperties).toBe(false);
  expect(Object.keys(audit.properties as JsonObject).sort()).toEqual(expectedProperties.sort());
  expect(JSON.stringify(audit).toLowerCase()).not.toContain('password');
  expect(JSON.stringify(audit).toLowerCase()).not.toContain('token');
});
