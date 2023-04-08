/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MediaLMListResponse } from '../models/MediaLMListResponse';
import type { MediaVM } from '../models/MediaVM';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class MediaService {

    /**
     * @returns MediaLMListResponse Success
     * @throws ApiError
     */
    public static getMedia({
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        size?: number,
        page?: number,
        ascending?: boolean,
        sortBy?: string,
        searchTerm?: string,
    }): CancelablePromise<MediaLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/media',
            query: {
                'size': size,
                'page': page,
                'ascending': ascending,
                'sortBy': sortBy,
                'searchTerm': searchTerm,
            },
        });
    }

    /**
     * @returns MediaVM Success
     * @throws ApiError
     */
    public static getMediaById({
        mediaId,
    }: {
        mediaId: number,
    }): CancelablePromise<MediaVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/media/{mediaId}',
            path: {
                'mediaId': mediaId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static deleteMedia({
        mediaId,
    }: {
        mediaId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/media/{mediaId}',
            path: {
                'mediaId': mediaId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

}
