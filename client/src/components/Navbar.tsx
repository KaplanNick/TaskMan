import { useNavigate } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
} from '@mui/material';

type View = 'tasks' | 'newTask' | 'newUser' | 'newTag';

interface NavbarProps {
  currentView: View;
}

const pathMap: Record<View, string> = {
  tasks: '/',
  newTask: '/new-task',
  newUser: '/new-user',
  newTag: '/new-tag',
};

export function Navbar({ currentView }: NavbarProps) {
  const navigate = useNavigate();

  const handleNavigation = (view: View) => {
    navigate(pathMap[view]);
  };

  return (
    <AppBar position="sticky">
      <Toolbar sx={{ gap: { xs: 0.5, sm: 1 } }}>
        <Typography
          variant="h6"
          component="div"
          sx={{
            flexGrow: 1,
            fontSize: { xs: '1rem', sm: '1.25rem' },
          }}
        >
          TaskMan
        </Typography>
        <Button
          color="inherit"
          onClick={() => handleNavigation('tasks')}
          variant={currentView === 'tasks' ? 'outlined' : 'text'}
          sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem' } }}
        >
          Tasks
        </Button>
        <Button
          color="inherit"
          onClick={() => handleNavigation('newTask')}
          variant={currentView === 'newTask' ? 'outlined' : 'text'}
          sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem' } }}
        >
          New Task
        </Button>
        <Button
          color="inherit"
          onClick={() => handleNavigation('newUser')}
          variant={currentView === 'newUser' ? 'outlined' : 'text'}
          sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem' } }}
        >
          New User
        </Button>
        <Button
          color="inherit"
          onClick={() => handleNavigation('newTag')}
          variant={currentView === 'newTag' ? 'outlined' : 'text'}
          sx={{ fontSize: { xs: '0.75rem', sm: '0.875rem' } }}
        >
          New Tag
        </Button>
      </Toolbar>
    </AppBar>
  );
}