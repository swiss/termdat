import {HttpErrorResponse, HttpInterceptorFn} from '@angular/common/http';
import {inject} from '@angular/core';
import {Router} from '@angular/router';
import {catchError, throwError} from 'rxjs';

export const authErrorInterceptor: HttpInterceptorFn = (req, next) => {
    const router = inject(Router);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // Handle 401 errors only for protected API calls. Public routes keep the user on search.
            const isPublicRoute = router.url === '/' || router.url.startsWith('/search');
            if (error.status === 401 && req.url.startsWith('/api/') && !isPublicRoute) {
                void router.navigate(['/unauthorized']);
            }

            return throwError(() => error);
        })
    );
};
