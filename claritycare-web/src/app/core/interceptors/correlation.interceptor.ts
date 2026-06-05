import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Correlation ID Interceptor
 * 
 * Attaches a unique X-Correlation-Id header to every outgoing HTTP request.
 * This ID is propagated through the entire backend pipeline (middleware → handlers → audit logs)
 * enabling full distributed tracing across the Angular SPA and ASP.NET Core API.
 * 
 * Compliance: SOC 2 Type II requires complete request traceability.
 */
export const correlationInterceptor: HttpInterceptorFn = (req, next) => {
  const correlationId = crypto.randomUUID().replace(/-/g, '');

  const cloned = req.clone({
    setHeaders: { 'X-Correlation-Id': correlationId }
  });

  return next(cloned);
};
