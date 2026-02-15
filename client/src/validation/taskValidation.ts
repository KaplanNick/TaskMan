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

  if (!title.trim()) {
    errors.title = 'Title is required';
  } else if (title.trim().length < 3) {
    errors.title = 'Title must be at least 3 characters';
  } else if (title.trim().length > 200) {
    errors.title = 'Title cannot exceed 200 characters';
  }

  if (!description.trim()) {
    errors.description = 'Description is required';
  } else if (description.trim().length < 10) {
    errors.description = 'Description must be at least 10 characters';
  } else if (description.trim().length > 2000) {
    errors.description = 'Description cannot exceed 2000 characters';
  }

  if (!dueDate) {
    errors.dueDate = 'Due date is required';
  } else {
    // Parse as UTC date (HTML date input returns date strings like "2025-02-20")
    const date = new Date(dueDate + 'T00:00:00Z');
    const today = new Date();
    today.setUTCHours(0, 0, 0, 0);
    const maxDate = new Date();
    maxDate.setUTCFullYear(maxDate.getUTCFullYear() + 10);
    maxDate.setUTCHours(0, 0, 0, 0);
    
    if (isNaN(date.getTime())) {
      errors.dueDate = 'Invalid date';
    } else if (date < today) {
      errors.dueDate = 'Due date cannot be in the past';
    } else if (date > maxDate) {
      errors.dueDate = 'Due date cannot be more than 10 years in the future';
    }
  }

  if (!priority || priority < 1 || priority > 3) {
    errors.priority = 'Priority is required';
  }

  if (!tagIds || tagIds.length === 0) {
    errors.tags = 'At least one tag is required';
  } else if (tagIds.length > 10) {
    errors.tags = 'Cannot assign more than 10 tags';
  }

  if (userId === '') {
    // If no user selected, validate new user fields
    if (!newUserFullName.trim()) {
      errors.newUserFullName = 'Full name is required';
    }
    if (!newUserEmail.trim()) {
      errors.newUserEmail = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(newUserEmail)) {
      errors.newUserEmail = 'Email is invalid';
    }
    if (!newUserTelephone.trim()) {
      errors.newUserTelephone = 'Telephone is required';
    } else if (!/^\+?[1-9]\d{0,14}([\s\-]?\d+)*$/.test(newUserTelephone.replace(/[\s\-\(\)]/g, ''))) {
      errors.newUserTelephone = 'Telephone is invalid';
    }
  }

  return errors;
};