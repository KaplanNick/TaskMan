/**
 * Common validation utilities and reusable validators
 * These match the backend validation rules for consistency
 */

/**
 * Validates if a string is not empty after trimming
 */
export const isNotEmpty = (value: string): boolean => {
  return value.trim().length > 0;
};

/**
 * Validates minimum length after trimming
 */
export const hasMinLength = (value: string, minLength: number): boolean => {
  return value.trim().length >= minLength;
};

/**
 * Validates maximum length after trimming
 */
export const hasMaxLength = (value: string, maxLength: number): boolean => {
  return value.trim().length <= maxLength;
};

/**
 * Validates email format using RFC 5322 simplified pattern
 */
export const isValidEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email.trim());
};

/**
 * Validates telephone format (international format)
 * Accepts: +1234567890, 123-456-7890, (123) 456-7890, etc.
 */
export const isValidPhone = (phone: string): boolean => {
  const phoneRegex = /^\+?[1-9]\d{0,14}([\s\-]?\d+)*$/;
  const cleanPhone = phone.replace(/[\s\-\(\)]/g, '');
  return phoneRegex.test(cleanPhone);
};

/**
 * Validates if date is not in the past
 */
export const isNotPastDate = (dateString: string): boolean => {
  const date = new Date(dateString + 'T00:00:00Z');
  const today = new Date();
  today.setUTCHours(0, 0, 0, 0);
  return date >= today;
};

/**
 * Validates if date is within a certain number of years from now
 */
export const isWithinYears = (dateString: string, years: number): boolean => {
  const date = new Date(dateString + 'T00:00:00Z');
  const maxDate = new Date();
  maxDate.setUTCFullYear(maxDate.getUTCFullYear() + years);
  maxDate.setUTCHours(0, 0, 0, 0);
  return date <= maxDate;
};

/**
 * Validates if a date string is valid
 */
export const isValidDate = (dateString: string): boolean => {
  const date = new Date(dateString + 'T00:00:00Z');
  return !isNaN(date.getTime());
};

/**
 * Generic field validator with common error messages
 */
export interface FieldValidator<T = string> {
  validate: (value: T) => string | undefined;
}

/**
 * Creates a required field validator
 */
export const required = (fieldName: string): FieldValidator => ({
  validate: (value: string) => {
    return isNotEmpty(value) ? undefined : `${fieldName} is required`;
  }
});

/**
 * Creates a min length validator
 */
export const minLength = (min: number): FieldValidator => ({
  validate: (value: string) => {
    return hasMinLength(value, min) 
      ? undefined 
      : `Must be at least ${min} characters`;
  }
});

/**
 * Creates a max length validator
 */
export const maxLength = (max: number): FieldValidator => ({
  validate: (value: string) => {
    return hasMaxLength(value, max) 
      ? undefined 
      : `Cannot exceed ${max} characters`;
  }
});

/**
 * Creates an email validator
 */
export const email = (): FieldValidator => ({
  validate: (value: string) => {
    return isValidEmail(value) ? undefined : 'Email is invalid';
  }
});

/**
 * Creates a phone validator
 */
export const phone = (): FieldValidator => ({
  validate: (value: string) => {
    return isValidPhone(value) ? undefined : 'Telephone is invalid';
  }
});

/**
 * Composes multiple validators into a single validator
 * Returns the first error found, or undefined if all pass
 */
export const composeValidators = (...validators: FieldValidator[]): FieldValidator => ({
  validate: (value: string) => {
    for (const validator of validators) {
      const error = validator.validate(value);
      if (error) return error;
    }
    return undefined;
  }
});

/**
 * Validates a single field with multiple validators
 */
export const validateField = (value: string, ...validators: FieldValidator[]): string | undefined => {
  const composed = composeValidators(...validators);
  return composed.validate(value);
};
