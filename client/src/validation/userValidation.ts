import { validateField, required, email, phone } from './validators';

export interface UserValidationErrors {
  fullName?: string;
  email?: string;
  telephone?: string;
}

export const validateUser = (
  fullName: string, 
  emailValue: string, 
  telephone: string
): UserValidationErrors => {
  const errors: UserValidationErrors = {};

  // Validate full name
  const fullNameError = validateField(fullName, required('Full name'));
  if (fullNameError) {
    errors.fullName = fullNameError;
  }

  // Validate email
  const emailError = validateField(emailValue, required('Email'), email());
  if (emailError) {
    errors.email = emailError;
  }

  // Validate telephone
  const telephoneError = validateField(telephone, required('Telephone'), phone());
  if (telephoneError) {
    errors.telephone = telephoneError;
  }

  return errors;
};