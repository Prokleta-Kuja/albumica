/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AuthStatusModel } from '../models/AuthStatusModel';
import type { LoginModel } from '../models/LoginModel';

import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';

export class AuthService {

    /**
     * @returns AuthStatusModel Success
     * @throws ApiError
     */
    public static status(): CancelablePromise<AuthStatusModel> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/auth',
        });
    }

    /**
     * @returns AuthStatusModel Success
     * @throws ApiError
     */
    public static autoLogin(): CancelablePromise<AuthStatusModel> {
        return __request(OpenAPI, {
            method: 'PATCH',
            url: '/api/auth',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns AuthStatusModel Success
     * @throws ApiError
     */
    public static login({
        requestBody,
    }: {
        requestBody?: LoginModel,
    }): CancelablePromise<AuthStatusModel> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/auth',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }

    /**
     * @returns void
     * @throws ApiError
     */
    public static logout(): CancelablePromise<void> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/auth',
            errors: {
                404: `Not Found`,
            },
        });
    }

}
