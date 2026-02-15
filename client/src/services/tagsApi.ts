import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";

export interface TagDto {
  id: number;
  name: string;
}

export interface CreateTagDto {
  name: string;
}

export const tagsApi = createApi({
  reducerPath: "tagsApi",
  baseQuery: fetchBaseQuery({ baseUrl: "http://localhost:5000/api" }),
  tagTypes: ["Tag"],
  endpoints: (builder) => ({
    getAllTags: builder.query<TagDto[], void>({
      query: () => "/tags",
      providesTags: ["Tag"],
    }),

    getTagById: builder.query<TagDto, number>({
      query: (id) => `/tags/${id}`,
      providesTags: (result, error, id) => [{ type: "Tag", id }],
    }),

    createTag: builder.mutation<TagDto, CreateTagDto>({
      query: (body) => ({
        url: "/tags",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Tag"],
    }),

    updateTag: builder.mutation<TagDto, { id: number; body: CreateTagDto }>({
      query: ({ id, body }) => ({
        url: `/tags/${id}`,
        method: "PUT",
        body,
      }),
      invalidatesTags: (result, error, { id }) => [{ type: "Tag", id }, "Tag"],
    }),

    deleteTag: builder.mutation<void, number>({
      query: (id) => ({
        url: `/tags/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["Tag"],
    }),
  }),
});

export const {
  useGetAllTagsQuery,
  useGetTagByIdQuery,
  useCreateTagMutation,
  useUpdateTagMutation,
  useDeleteTagMutation,
} = tagsApi;