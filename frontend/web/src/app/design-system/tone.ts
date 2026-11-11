export type Tone = 'success' | 'warning' | 'error' | 'info' | 'accent' | 'neutral';

export const tones: Record<Tone, { icon: string; word: string }> = {
  success: { icon: 'check', word: 'Sucesso' },
  warning: { icon: 'alert', word: 'Atenção' },
  error: { icon: 'alert', word: 'Erro' },
  info: { icon: 'info', word: 'Informação' },
  accent: { icon: 'info', word: 'Informação' },
  neutral: { icon: 'info', word: 'Estado' },
};
