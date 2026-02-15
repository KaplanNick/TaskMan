import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateTaskMutation } from '../services/tasksApi';
import { useGetAllUsersQuery, useCreateUserMutation } from '../services/usersApi';
import { useGetAllTagsQuery } from '../services/tagsApi';
import { validateTask, type TaskValidationErrors } from '../validation/taskValidation';
import { getErrorMessage } from '../types/api';
import {
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Chip,
  OutlinedInput,
  Box,
  Typography,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';
import { BaseForm } from '../components/BaseForm';
import { FormAlerts } from '../components/FormAlerts';
import { FormActions } from '../components/FormActions';

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 250,
    },
  },
};

export function NewTaskForm() {
  const navigate = useNavigate();
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [dueDate, setDueDate] = useState('');
  const [priority, setPriority] = useState<number>(1);
  const [selectedUserId, setSelectedUserId] = useState<number | ''>('');
  const [selectedTags, setSelectedTags] = useState<number[]>([]);
  const [newUserFullName, setNewUserFullName] = useState('');
  const [newUserEmail, setNewUserEmail] = useState('');
  const [newUserTelephone, setNewUserTelephone] = useState('');
  const [errors, setErrors] = useState<TaskValidationErrors>({});

  const [createTask, { isLoading: isCreatingTask, isSuccess, isError, error }] = useCreateTaskMutation();
  const [createUser, { isLoading: isCreatingUser }] = useCreateUserMutation();
  const { data: users } = useGetAllUsersQuery();
  const { data: tags } = useGetAllTagsQuery();

  // Navigate after showing success message
  useEffect(() => {
    if (isSuccess) {
      const timer = setTimeout(() => {
        navigate('/');
      }, 1500);
      return () => clearTimeout(timer);
    }
  }, [isSuccess, navigate]);

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const validationErrors = validateTask(
      title,
      description,
      dueDate,
      priority,
      selectedUserId,
      selectedTags,
      newUserFullName,
      newUserEmail,
      newUserTelephone
    );
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});

    let userId = selectedUserId;

    if (userId === '') {
      // Check for duplicate email or telephone before creating user
      const duplicateErrors: any = {};
      
      const emailExists = users?.some(
        user => user.email.toLowerCase() === newUserEmail.toLowerCase()
      );
      if (emailExists) {
        duplicateErrors.newUserEmail = `A user with the email '${newUserEmail}' already exists.`;
      }

      const telephoneExists = users?.some(
        user => user.telephone === newUserTelephone
      );
      if (telephoneExists) {
        duplicateErrors.newUserTelephone = `A user with the telephone '${newUserTelephone}' already exists.`;
      }

      if (Object.keys(duplicateErrors).length > 0) {
        setErrors(duplicateErrors);
        return;
      }

      // Create new user first
      try {
        const newUser = await createUser({
          fullName: newUserFullName,
          email: newUserEmail,
          telephone: newUserTelephone,
        }).unwrap();
        userId = newUser.id as number;
      } catch (err) {
        const errorMessage = getErrorMessage(err);
        const mappedErrors: any = {};

        if (errorMessage.toLowerCase().includes('email')) {
          mappedErrors.newUserEmail = errorMessage;
        } else if (errorMessage.toLowerCase().includes('telephone')) {
          mappedErrors.newUserTelephone = errorMessage;
        } else {
          mappedErrors.newUserFullName = errorMessage;
        }

        setErrors(prev => ({ ...prev, ...mappedErrors }));
        return;
      }
    }

    try {
      await createTask({
        title,
        description,
        dueDate,
        priority,
        userId: Number(userId),
        tagIds: selectedTags,
      }).unwrap();

      // Reset form on success
      setTitle('');
      setDescription('');
      setDueDate('');
      setPriority(1);
      setSelectedUserId('');
      setSelectedTags([]);
      setNewUserFullName('');
      setNewUserEmail('');
      setNewUserTelephone('');
    } catch (err) {
      // Error handled by RTK Query
    }
  };

  const handleTagChange = (event: SelectChangeEvent<number[]>) => {
    const { target: { value } } = event;
    setSelectedTags(typeof value === 'string' ? value.split(',').map(Number) : value);
  };

  const handleClear = () => {
    setTitle('');
    setDescription('');
    setDueDate('');
    setPriority(1);
    setSelectedUserId('');
    setSelectedTags([]);
    setNewUserFullName('');
    setNewUserEmail('');
    setNewUserTelephone('');
    setErrors({});
  };

  const isLoading = isCreatingTask || isCreatingUser;

  return (
    <BaseForm title="Create New Task" onSubmit={handleSubmit} maxWidth={600}>
      <FormAlerts
        isSuccess={isSuccess}
        isError={isError}
        successMessage="Task created successfully!"
        errorMessage={`Failed to create task: ${getErrorMessage(error)}`}
      />

      <TextField
        fullWidth
        label="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={!!errors.title}
        helperText={errors.title || `${title.trim().length}/200 characters (min 3)`}
        margin="normal"
        required
        disabled={isLoading}
        slotProps={{ htmlInput: { maxLength: 200 } }}
      />

      <TextField
        fullWidth
        label="Description"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        error={!!errors.description}
        helperText={errors.description || `${description.trim().length}/2000 characters (min 10)`}
        margin="normal"
        multiline
        rows={3}
        required
        disabled={isLoading}
        slotProps={{ htmlInput: { maxLength: 2000 } }}
      />

      <TextField
        fullWidth
        label="Due Date"
        type="date"
        value={dueDate}
        onChange={(e) => setDueDate(e.target.value)}
        error={!!errors.dueDate}
        helperText={errors.dueDate}
        margin="normal"
        required
        disabled={isLoading}
        InputLabelProps={{ shrink: true }}
      />

      <FormControl fullWidth margin="normal" error={!!errors.priority}>
        <InputLabel>Priority</InputLabel>
        <Select
          value={priority}
          label="Priority"
          onChange={(e) => setPriority(Number(e.target.value))}
          disabled={isLoading}
        >
          <MenuItem value={1}>Low</MenuItem>
          <MenuItem value={2}>Medium</MenuItem>
          <MenuItem value={3}>High</MenuItem>
        </Select>
        {errors.priority && <FormHelperText>{errors.priority}</FormHelperText>}
      </FormControl>

      <FormControl fullWidth margin="normal" error={!!errors.userId}>
        <InputLabel>User</InputLabel>
        <Select
          value={selectedUserId}
          label="User"
          onChange={(e) => setSelectedUserId(e.target.value ? Number(e.target.value) : '')}
          disabled={isLoading}
        >
          <MenuItem value="">
            <em>Select User</em>
          </MenuItem>
          {users?.map((user) => (
            <MenuItem key={user.id} value={user.id}>
              {user.fullName} ({user.email})
            </MenuItem>
          ))}
        </Select>
        {errors.userId && <FormHelperText>{errors.userId}</FormHelperText>}
      </FormControl>

      <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>
        Or Create New User
      </Typography>

      <TextField
        fullWidth
        label="Full Name"
        value={newUserFullName}
        onChange={(e) => setNewUserFullName(e.target.value)}
        error={!!errors.newUserFullName}
        helperText={errors.newUserFullName}
        margin="normal"
        disabled={selectedUserId !== '' || isLoading}
      />

      <TextField
        fullWidth
        label="Phone"
        value={newUserTelephone}
        onChange={(e) => setNewUserTelephone(e.target.value)}
        error={!!errors.newUserTelephone}
        helperText={errors.newUserTelephone}
        margin="normal"
        disabled={selectedUserId !== '' || isLoading}
      />

      <TextField
        fullWidth
        label="Email"
        type="email"
        value={newUserEmail}
        onChange={(e) => setNewUserEmail(e.target.value)}
        error={!!errors.newUserEmail}
        helperText={errors.newUserEmail}
        margin="normal"
        disabled={selectedUserId !== '' || isLoading}
      />

      <FormControl fullWidth margin="normal" error={!!errors.tags}>
        <InputLabel>Tags</InputLabel>
        <Select
          multiple
          value={selectedTags}
          onChange={handleTagChange}
          input={<OutlinedInput label="Tags" />}
          disabled={isLoading}
          renderValue={(selected) => (
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
              {selected.map((tagId) => {
                const tag = tags?.find((t) => t.id === tagId);
                return <Chip key={tagId} label={tag?.name || tagId} />;
              })}
            </Box>
          )}
          MenuProps={MenuProps}
        >
          {tags?.map((tag) => (
            <MenuItem key={tag.id} value={tag.id}>
              {tag.name}
            </MenuItem>
          ))}
        </Select>
        {errors.tags && <FormHelperText>{errors.tags}</FormHelperText>}
      </FormControl>

      <FormActions
        isLoading={isLoading}
        submitLabel="Save"
        loadingLabel="Saving..."
        onClear={handleClear}
        onCancel={() => navigate('/')}
      />
    </BaseForm>
  );
}
