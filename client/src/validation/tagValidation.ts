import { validateField, required, minLength, maxLength } from './validators';

export interface TagValidationErrors {
  name?: string;
}

export const validateTag = (
  name: string, 
  existingTags: Array<{ id: number; name: string }> = []
): TagValidationErrors => {
  const errors: TagValidationErrors = {};

  // Validate name with composed validators
  const nameError = validateField(
    name,
    required('Name'),
    minLength(2),
    maxLength(50)
  );

  if (nameError) {
    errors.name = nameError;
  } else if (existingTags.some(tag => tag.name.toLowerCase() === name.trim().toLowerCase())) {
    errors.name = 'A tag with this name already exists';
  }

  return errors;
};