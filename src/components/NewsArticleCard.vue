<script setup lang="ts">
import type { NewsArticle } from '../types/polygon'

defineProps<{
  article: NewsArticle
}>()

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
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
  <div class="bg-white rounded-lg shadow-sm hover:shadow-md transition-shadow p-4 flex gap-4 border border-solid border-indigo-600">
    <!-- Publisher Logo -->
    <div class="flex-shrink-0">
      <img 
        v-if="article.publisher.favicon_url"
        :src="article.publisher.favicon_url" 
        :alt="article.publisher.name"
        class="w-6 h-6 rounded-full"
      />
    </div>

    <!-- Article Content -->
    <div class="flex-grow min-w-0">
      <!-- Header -->
      <div class="flex items-center justify-between gap-2 mb-1">
        <span class="text-sm text-gray-600 truncate">{{ article.publisher.name }}</span>
        <span class="text-sm text-gray-500 flex-shrink-0">{{ formatDate(article.published_utc) }}</span>
      </div>

      <!-- Title and Description -->
      <h3 class="text-base font-semibold mb-1 line-clamp-2">{{ article.title }}</h3>
      <p class="text-sm text-gray-600 mb-2 line-clamp-2">{{ article.description }}</p>

      <!-- Footer -->
      <div class="flex items-center justify-between gap-2">
        <!-- Sentiment Tags -->
        <div class="flex gap-2 overflow-x-auto">
          <span 
            v-for="insight in article.insights" 
            :key="insight.ticker"
            :class="[
              'text-xs px-2 py-0.5 rounded-full whitespace-nowrap',
              getSentimentColor(insight.sentiment)
            ]"
          >
            {{ insight.ticker }}: {{ insight.sentiment }}
          </span>
        </div>

        <!-- Read More Link -->
        <a 
          :href="article.article_url" 
          target="_blank" 
          rel="noopener noreferrer"
          class="text-indigo-600 hover:text-indigo-800 text-sm font-medium flex-shrink-0"
        >
          Read →
        </a>
      </div>
    </div>
  </div>
</template>