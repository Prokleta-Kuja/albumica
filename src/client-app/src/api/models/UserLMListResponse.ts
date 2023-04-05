/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { UserLM } from './UserLM';

export type UserLMListResponse = {
    items: Array<UserLM>;
    size: number;
    page: number;
    total: number;
    ascending: boolean;
    sortBy?: string | null;
};

