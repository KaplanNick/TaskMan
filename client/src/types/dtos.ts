/**
 * Shared DTO types across all API services
 * These match the backend DTOs exactly
 */

export interface TagDto {
  id: number;
  name: string;
}

export interface CreateTagDto {
  name: string;
}

export interface UpdateTagDto {
  name: string;
}

export interface UserDto {
  id: number;
  fullName: string;
  email: string;
  telephone: string;
}

export interface CreateUserDto {
  fullName: string;
  email: string;
  telephone: string;
}

export interface UpdateUserDto {
  fullName: string;
  email: string;
  telephone: string;
}

export interface TaskDto {
  id: number;
  title: string;
  description: string;
  dueDate: string;
  priority: number;
  userId: number;
  tags: TagDto[];
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: number;
  userId: number;
  tagIds: number[];
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: number;
  tagIds: number[];
}

/**
 * Task Priority enum matching backend
 */
export const TaskPriority = {
  Low: 0,
  Medium: 1,
  High: 2,
} as const;

export type TaskPriority = typeof TaskPriority[keyof typeof TaskPriority];

export const TaskPriorityLabels = {
  0: "Low",
  1: "Medium",
  2: "High",
};
