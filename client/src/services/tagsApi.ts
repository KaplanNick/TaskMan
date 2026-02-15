import { baseApi } from "./baseApi";
import type { TagDto, CreateTagDto, UpdateTagDto } from "../types/dtos";

export const tagsApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getAllTags: builder.query<TagDto[], void>({
      query: () => "/tags",
      providesTags: ["Tag"],
    }),

    getTagById: builder.query<TagDto, number>({
      query: (id) => `/tags/${id}`,
      // @ts-ignore - unused params required by RTK Query signature
      providesTags: (_, __, id) => [{ type: "Tag", id }],
    }),

    createTag: builder.mutation<TagDto, CreateTagDto>({
      query: (body) => ({
        url: "/tags",
        method: "POST",
        body,
      }),
      invalidatesTags: ["Tag"],
    }),

    updateTag: builder.mutation<TagDto, { id: number; body: UpdateTagDto }>({
      query: ({ id, body }) => ({
        url: `/tags/${id}`,
        method: "PUT",
        body,
      }),
      // @ts-ignore - unused params required by RTK Query signature
      invalidatesTags: (_, __, { id }) => [{ type: "Tag", id }, "Tag"],
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