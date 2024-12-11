<script setup lang="ts">
import type { NewsArticle } from '../types/polygon'

defineProps<{
  article: NewsArticle
}>()

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getSentimentColor = (sentiment: string): string => {
  switch (sentiment.toLowerCase()) {
    case 'positive':
      return 'bg-green-100 text-green-800'
    case 'negative':
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}
</script>

<template>
  <div class="bg-white rounded-lg shadow-md overflow-hidden">
    <div class="relative">
      <img 
        v-if="article.image_url"
        :src="article.image_url" 
        :alt="article.title"
        class="w-full h-48 object-cover"
      />
      <div class="absolute top-0 left-0 p-2">
        <img 
          v-if="article.publisher.favicon_url"
          :src="article.publisher.favicon_url" 
          :alt="article.publisher.name"
          class="w-6 h-6 rounded-full bg-white"
        />
      </div>
    </div>
    <div class="p-4">
      <div class="flex items-center justify-between mb-2">
        <span class="text-sm text-gray-500">{{ article.publisher.name }}</span>
        <span class="text-sm text-gray-500">{{ formatDate(article.published_utc) }}</span>
      </div>
      <h3 class="text-lg font-semibold mb-2">{{ article.title }}</h3>
      <p class="text-gray-600 text-sm mb-4">{{ article.description }}</p>
      <div class="space-y-3">
        <div v-if="article.insights?.length" class="space-y-2">
          <div 
            v-for="insight in article.insights" 
            :key="insight.ticker"
            :class="[
              'text-sm px-3 py-1 rounded-full inline-block mr-2',
              getSentimentColor(insight.sentiment)
            ]"
          >
            {{ insight.ticker }}: {{ insight.sentiment }}
          </div>
        </div>
        <div class="flex justify-between items-center">
          <div class="space-x-2">
            <span 
              v-for="keyword in article.keywords.slice(0, 3)" 
              :key="keyword"
              class="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded"
            >
              {{ keyword }}
            </span>
          </div>
          <a 
            :href="article.article_url" 
            target="_blank" 
            rel="noopener noreferrer"
            class="text-indigo-600 hover:text-indigo-800 text-sm font-medium"
          >
            Read More →
          </a>
        </div>
      </div>
    </div>
  </div>
</template>