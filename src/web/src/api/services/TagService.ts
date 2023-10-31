/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { TagCM } from '../models/TagCM';
import type { TagLMListResponse } from '../models/TagLMListResponse';
import type { TagUM } from '../models/TagUM';
import type { TagVM } from '../models/TagVM';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class TagService {

    /**
     * @returns TagLMListResponse Success
     * @throws ApiError
     */
    public static getTags({
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
    }): CancelablePromise<TagLMListResponse> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tags',
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
     * @returns TagVM Success
     * @throws ApiError
     */
    public static createTag({
        requestBody,
    }: {
        requestBody?: TagCM,
    }): CancelablePromise<TagVM> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/tags',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns TagVM Success
     * @throws ApiError
     */
    public static getTag({
        tagId,
    }: {
        tagId: number,
    }): CancelablePromise<TagVM> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/tags/{tagId}',
            path: {
                'tagId': tagId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

    /**
     * @returns TagVM Success
     * @throws ApiError
     */
    public static updateTag({
        tagId,
        requestBody,
    }: {
        tagId: number,
        requestBody?: TagUM,
    }): CancelablePromise<TagVM> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/tags/{tagId}',
            path: {
                'tagId': tagId,
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
    public static deleteTag({
        tagId,
    }: {
        tagId: number,
    }): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/tags/{tagId}',
            path: {
                'tagId': tagId,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }

}
