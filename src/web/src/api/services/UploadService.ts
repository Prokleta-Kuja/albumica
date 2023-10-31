/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class UploadService {

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static uploadFiles(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/uploads',
        });
    }

    /**
     * @returns any Success
     * @throws ApiError
     */
    public static processQueue(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/uploads',
        });
    }

}
