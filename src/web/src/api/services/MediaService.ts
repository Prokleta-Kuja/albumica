/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { MediaLMListResponse } from '../models/MediaLMListResponse';
import type { MediaUM } from '../models/MediaUM';
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
        inBasket,
        hidden,
        noCreate,
        tagIds,
        size,
        page,
        ascending,
        sortBy,
        searchTerm,
    }: {
        inBasket?: boolean,
        hidden?: boolean,
        noCreate?: boolean,
        tagIds?: Array<number>,
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
                'inBasket': inBasket,
                'hidden': hidden,
                'noCreate': noCreate,
                'tagIds': tagIds,
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
     * @returns MediaVM Success
     * @throws ApiError
     */
    public static updateMedia({
        mediaId,
        requestBody,
    }: {
        mediaId: number,
        requestBody?: MediaUM,
    }): CancelablePromise<MediaVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/media/{mediaId}',
            path: {
                'mediaId': mediaId,
            },
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
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

    /**
     * @returns number Success
     * @throws ApiError
     */
    public static getBasketItems(): CancelablePromise<Array<number>> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/media/basket',
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static addToBasket({
        mediaId,
    }: {
        mediaId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/media/{mediaId}/basket',
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
    public static removeFromBasket({
        mediaId,
    }: {
        mediaId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/media/{mediaId}/basket',
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
    public static addTag({
        mediaId,
        tagId,
    }: {
        mediaId: number,
        tagId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/media/{mediaId}/tags/{tagId}',
            path: {
                'mediaId': mediaId,
                'tagId': tagId,
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
    public static removeTag({
        mediaId,
        tagId,
    }: {
        mediaId: number,
        tagId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/media/{mediaId}/tags/{tagId}',
            path: {
                'mediaId': mediaId,
                'tagId': tagId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getApiMedia({
        path,
    }: {
        path: string,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/media/{path}',
            path: {
                'path': path,
            },
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static getApiMediaZip({
        bundleId,
    }: {
        bundleId: number,
    }): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/media/zip/{bundleId}',
            path: {
                'bundleId': bundleId,
            },
        });
    }

}
