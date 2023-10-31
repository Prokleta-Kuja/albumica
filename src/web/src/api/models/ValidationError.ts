/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PlainError } from './PlainError';

export type ValidationError = (PlainError & {
    errors?: Record<string, string>;
});

