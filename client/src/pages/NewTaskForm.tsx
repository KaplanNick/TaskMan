import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateTaskMutation } from '../services/tasksApi';
import { useGetAllUsersQuery, useCreateUserMutation } from '../services/usersApi';
import { useGetAllTagsQuery } from '../services/tagsApi';
import { validateTask, type TaskValidationErrors } from '../validation/taskValidation';
import {
  TextField,
  Button,
  Box,
  Typography,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Chip,
  OutlinedInput,
} from '@mui/material';
import type { SelectChangeEvent } from '@mui/material';

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
      // Create new user first
      try {
        const newUser = await createUser({
          fullName: newUserFullName,
          email: newUserEmail,
          telephone: newUserTelephone,
        }).unwrap();
        userId = newUser.id;
      } catch (err) {
        return; // Error handled by RTK Query
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

      // Navigate back to tasks
      navigate('/');
    } catch (err) {
      // Error handled by RTK Query
    }
  };

  const handleTagChange = (event: SelectChangeEvent<number[]>) => {
    const {
      target: { value },
    } = event;
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

  return (
    <Box sx={{ display: 'flex', justifyContent: 'center', width: '100%' }}>
      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{
          maxWidth: { xs: '100%', sm: 600 },
          width: '100%',
          p: { xs: 2, sm: 3 },
        }}
      >
        <Typography variant="h5" component="h1" gutterBottom>
          Create New Task
        </Typography>

      {isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Task created successfully!
        </Alert>
      )}

      {isError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          Failed to create task: {(error as any)?.data?.message || (error as any)?.message || 'Unknown error'}
        </Alert>
      )}

      <TextField
        fullWidth
        label="Title"
        value={title}
        onChange={(e) => setTitle(e.target.value)}
        error={!!errors.title}
        helperText={errors.title || `${title.trim().length}/200 characters (min 3)`}
        margin="normal"
        required
        inputProps={{ maxLength: 200 }}
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
        inputProps={{ maxLength: 2000 }}
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
        InputLabelProps={{
          shrink: true,
        }}
      />

      <FormControl fullWidth margin="normal" error={!!errors.priority}>
        <InputLabel>Priority</InputLabel>
        <Select
          value={priority}
          label="Priority"
          onChange={(e) => setPriority(Number(e.target.value))}
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
        disabled={selectedUserId !== ''}
      />

      <TextField
        fullWidth
        label="Phone"
        value={newUserTelephone}
        onChange={(e) => setNewUserTelephone(e.target.value)}
        error={!!errors.newUserTelephone}
        helperText={errors.newUserTelephone}
        margin="normal"
        disabled={selectedUserId !== ''}
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
        disabled={selectedUserId !== ''}
      />

      <FormControl fullWidth margin="normal" error={!!errors.tags}>
        <InputLabel>Tags</InputLabel>
        <Select
          multiple
          value={selectedTags}
          onChange={handleTagChange}
          input={<OutlinedInput label="Tags" />}
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

      <Box
        sx={{
          display: 'flex',
          flexDirection: { xs: 'column', sm: 'row' },
          gap: 2,
          mt: 3,
          mb: 2,
        }}
      >
        <Button
          type="submit"
          variant="contained"
          disabled={isCreatingTask || isCreatingUser}
          sx={{ flex: 1 }}
        >
          {isCreatingTask || isCreatingUser ? 'Saving...' : 'Save'}
        </Button>
        <Button
          type="button"
          variant="outlined"
          onClick={handleClear}
          sx={{ flex: 1 }}
        >
          Clear
        </Button>
        <Button
          type="button"
          variant="text"
          onClick={() => navigate('/')}
          sx={{ flex: 1 }}
        >
          Cancel
        </Button>
      </Box>
      </Box>
    </Box>
  );
}
