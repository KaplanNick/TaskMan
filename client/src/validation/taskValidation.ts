import { 
  validateField, 
  required, 
  minLength, 
  maxLength, 
  email, 
  phone,
  isValidDate,
  isNotPastDate,
  isWithinYears
} from './validators';

export interface TaskValidationErrors {
  title?: string;
  description?: string;
  dueDate?: string;
  priority?: string;
  userId?: string;
  tags?: string;
  newUserFullName?: string;
  newUserEmail?: string;
  newUserTelephone?: string;
}

export const validateTask = (
  title: string,
  description: string,
  dueDate: string,
  priority: number,
  userId: number | '',
  tagIds: number[],
  newUserFullName: string,
  newUserEmail: string,
  newUserTelephone: string
): TaskValidationErrors => {
  const errors: TaskValidationErrors = {};

  // Validate title
  const titleError = validateField(
    title,
    required('Title'),
    minLength(3),
    maxLength(200)
  );
  if (titleError) {
    errors.title = titleError;
  }

  // Validate description
  const descriptionError = validateField(
    description,
    required('Description'),
    minLength(10),
    maxLength(2000)
  );
  if (descriptionError) {
    errors.description = descriptionError;
  }

  // Validate due date
  if (!dueDate) {
    errors.dueDate = 'Due date is required';
  } else if (!isValidDate(dueDate)) {
    errors.dueDate = 'Invalid date';
  } else if (!isNotPastDate(dueDate)) {
    errors.dueDate = 'Due date cannot be in the past';
  } else if (!isWithinYears(dueDate, 10)) {
    errors.dueDate = 'Due date cannot be more than 10 years in the future';
  }

  // Validate priority
  if (!priority || priority < 1 || priority > 3) {
    errors.priority = 'Priority is required';
  }

  // Validate tags
  if (!tagIds || tagIds.length === 0) {
    errors.tags = 'At least one tag is required';
  } else if (tagIds.length > 10) {
    errors.tags = 'Cannot assign more than 10 tags';
  }

  // Validate user selection or new user fields
  if (userId === '') {
    // If no user selected, validate new user fields
    const fullNameError = validateField(newUserFullName, required('Full name'));
    if (fullNameError) {
      errors.newUserFullName = fullNameError;
    }

    const emailError = validateField(newUserEmail, required('Email'), email());
    if (emailError) {
      errors.newUserEmail = emailError;
    }

    const phoneError = validateField(newUserTelephone, required('Telephone'), phone());
    if (phoneError) {
      errors.newUserTelephone = phoneError;
    }
  }

  return errors;
};