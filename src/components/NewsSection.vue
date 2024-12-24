<script setup lang="ts">
import { computed, ref } from 'vue'
import type { NewsArticle } from '../types/polygon'

const props = defineProps<{
  articles: NewsArticle[]
  companyName: string
  symbol: string
}>()

const isExpanded = ref(true)
const showAllNews = ref(false)

const displayedArticles = computed(() => {
  return showAllNews.value ? props.articles : props.articles.slice(0, 1)
})

const toggleExpanded = () => {
  isExpanded.value = !isExpanded.value
}

const toggleShowAll = () => {
  showAllNews.value = !showAllNews.value
}
</script>

<template>
  <div class="bg-white rounded-lg shadow-md p-6">
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-xl font-semibold">
        News for {{ companyName }} ({{ symbol }}) over past 30 days
      </h2>
      <button 
        @click="toggleExpanded"
        class="px-4 py-1 text-sm font-medium text-gray-600 hover:text-gray-900 bg-gray-200 hover:bg-gray-300 rounded-md transition-colors"
      >
        {{ isExpanded ? 'Hide News' : 'Show News' }}
      </button>
    </div>

    <div v-show="isExpanded" class="space-y-4">
      <div v-if="articles.length === 0" class="text-center text-gray-500">
        No recent news articles found
      </div>
      
      <template v-else>
        <NewsArticleCard
          v-for="article in displayedArticles"
          :key="article.id"
          :article="article"
        />

        <div v-if="articles.length > 1" class="text-center mt-6">
          <button
            @click="toggleShowAll"
            class="px-6 py-2 text-sm font-medium text-indigo-600 hover:text-indigo-800 bg-indigo-50 hover:bg-indigo-100 rounded-md transition-colors"
          >
            {{ showAllNews ? 'Show Less' : `Show All (${articles.length}) Articles` }}
          </button>
        </div>
      </template>
    </div>
  </div>
</template>