export interface UserValidationErrors {
  fullName?: string;
  email?: string;
  telephone?: string;
}

export const validateUser = (fullName: string, email: string, telephone: string): UserValidationErrors => {
  const errors: UserValidationErrors = {};

  if (!fullName.trim()) {
    errors.fullName = 'Full name is required';
  }

  if (!email.trim()) {
    errors.email = 'Email is required';
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errors.email = 'Email is invalid';
  }

  if (!telephone.trim()) {
    errors.telephone = 'Telephone is required';
  } else if (!/^\+?[\d\s\-\(\)]+$/.test(telephone)) {
    errors.telephone = 'Telephone is invalid';
  }

  return errors;
};