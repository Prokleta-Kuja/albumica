/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { MediaLM } from './MediaLM';

export type MediaLMListResponse = {
    items: Array<MediaLM>;
    size: number;
    page: number;
    total: number;
    ascending: boolean;
    sortBy?: string | null;
};

