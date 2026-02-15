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
  } else if (!/^\+?[1-9]\d{0,14}([\s\-]?\d+)*$/.test(telephone.replace(/[\s\-\(\)]/g, ''))) {
    errors.telephone = 'Telephone is invalid';
  }

  return errors;
};