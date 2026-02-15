/**
 * API Error Response Types
 * Matches the backend BaseApiController error format
 */

export interface ApiErrorResponse {
  data?: {
    message?: string;
    errors?: Record<string, string[]>;
  };
  message?: string;
  status?: number;
}

/**
 * Standard API error format from backend
 */
export interface ApiError {
  message: string;
}

/**
 * Extract error message from RTK Query error response
 * @param error - The error object from RTK Query
 * @returns User-friendly error message
 */
export function getErrorMessage(error: unknown): string {
  const apiError = error as ApiErrorResponse;

  // Try to get message from error.data.message (API validation error)
  if (apiError?.data?.message) {
    return apiError.data.message;
  }

  // Try to get message from error.message (generic error)
  if (apiError?.message) {
    return apiError.message;
  }

  // Fallback message
  return 'Unknown error occurred';
}

/**
 * Extract validation errors from API response
 * @param error - The error object from RTK Query
 * @returns Record of field names to error messages
 */
export function getValidationErrors(
  error: unknown
): Record<string, string> {
  const apiError = error as ApiErrorResponse;
  const errors: Record<string, string> = {};

  if (apiError?.data?.errors) {
    Object.entries(apiError.data.errors).forEach(([field, messages]) => {
      errors[field] = Array.isArray(messages) ? messages[0] : String(messages);
    });
  }

  return errors;
}
