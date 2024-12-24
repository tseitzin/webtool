```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { polygonService } from '../services/polygonService'
import { formatNumber } from '../utils/formatters'
import type { CompanyDetails, NewsArticle, StockData, RelatedCompany } from '../types/polygon'
import StockDetailsCard from '../components/StockDetailsCard.vue'
import RelatedCompanies from '../components/RelatedCompanies.vue'
import NewsSection from '../components/NewsSection.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const companyDetails = ref<CompanyDetails | null>(null)
const newsArticles = ref<NewsArticle[]>([])
const stockData = ref<StockData | null>(null)
const relatedCompanies = ref<RelatedCompany[]>([])
const loading = ref(true)
const error = ref('')

onMounted(async () => {
  if (!auth.isAuthenticated) {
    router.push('/login')
    return
  }

  const symbol = route.params.symbol as string
  if (!symbol) {
    router.push('/search-area')
    return
  }

  try {
    const [details, news, stock, related] = await Promise.all([
      polygonService.getCompanyDetails(symbol),
      polygonService.getCompanyNews(symbol),
      polygonService.getStockSnapshot(symbol),
      polygonService.getRelatedCompanies(symbol)])

    companyDetails.value = details
    newsArticles.value = news
    stockData.value = stock
    relatedCompanies.value = related
  } catch (e) {
    error.value = 'Failed to load company information'
    console.error(e)
  } finally {
    loading.value = false
  }
})

const formatMarketCap = (marketCap: number): string => {
  if (marketCap >= 1e12) {
    return `${(marketCap / 1e12).toFixed(2)}T`
  } else if (marketCap >= 1e9) {
    return `${(marketCap / 1e9).toFixed(2)}B`
  } else if (marketCap >= 1e6) {
    return `${(marketCap / 1e6).toFixed(2)}M`
  }
  return formatNumber(marketCap)
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const navigateToSearch = () => {
  router.push('/search-area')
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-8">
      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
        {{ error }}
      </div>

      <!-- Company Details -->
      <div v-else-if="companyDetails" class="space-y-6">
        <!-- Header Section -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <div class="flex items-start justify-between">
            <div>
              <h1 class="text-2xl text-gray-900">Company Name:  {{ companyDetails.name }}</h1>
              <p class="text-xl font-bold text-gray-800">Stock Symbol:  {{ companyDetails.ticker }}</p>
            </div>
            <div>
                <button
                    @click="navigateToSearch"
                    class="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors"
                >
                    Return to Search
                </button>
            </div>
          </div>
        </div>

        <!-- Stock Details Section -->
        <StockDetailsCard
          v-if="stockData"
          :stock="stockData"
        />

        <!-- Related Companies Section -->
         <div v-if="relatedCompanies">
            <RelatedCompanies
              :companies="relatedCompanies"
              :current-symbol="companyDetails.ticker"
            />
          </div>
          <div v-else class="font-semibold text-lg bg-white rounded-lg shadow-md p-3">
            No related companies
          </div>

        <!-- Key Statistics -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <h2 class="text-xl font-semibold mb-1">Key Statistics</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2">
            <div>
              <p class="text-md text-gray-500">Market Cap</p>
              <p class="text-md">{{ formatMarketCap(companyDetails.market_cap) }}</p>
            </div>
            <div>
              <p class="text-md text-gray-500">Employees</p>
              <p class="text-md">{{ formatNumber(companyDetails.total_employees) }}</p>
            </div>
            <div>
              <p class="text-md text-gray-500">Listed Since</p>
              <p class="text-md">{{ formatDate(companyDetails.list_date) }}</p>
            </div>
            <div>
              <p class="text-md text-gray-500">Exchange</p>
              <p class="text-md">{{ companyDetails.primary_exchange }}</p>
            </div>
            <div>
              <p class="text-md text-gray-500">Industry</p>
              <p class="text-sm">{{ companyDetails.sic_description }}</p>
            </div>
            <div>
              <p class="text-md text-gray-500">Outstanding Shares</p>
              <p class="text-md">{{ formatNumber(companyDetails.weighted_shares_outstanding) }}</p>
            </div>
          </div>
        </div>

        <!-- Company Description -->
        <div class="bg-white rounded-lg shadow-md p-6">
          <h2 class="text-xl font-semibold mb-2">About {{ companyDetails.name }}</h2>
          <p class="text-gray-700 leading-relaxed">{{ companyDetails.description }}</p>
          <div class="mt-4 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-2 items-center">
            <div>
              <p class="text-md text-gray-500">Headquarters</p>
              <p class="text-gray-700 text-sm">{{ companyDetails.address.address1 }}</p>
              <p class="text-gray-700 text-sm">
                {{ companyDetails.address.city }}, {{ companyDetails.address.state }} 
                {{ companyDetails.address.postal_code }}
              </p>
            </div>
            <div>
              <p class="text-md text-gray-500">Phone</p>
              <p class="text-gray-700 text-sm">{{ companyDetails.phone_number }}</p>
            </div>
            <div>
              <p class="text-md text-gray-500">Website</p>
              <a 
                :href="companyDetails.homepage_url"
                target="_blank"
                rel="noopener noreferrer"
                class="text-indigo-600 text-sm hover:text-indigo-800"
              >
                {{ companyDetails.homepage_url }}
              </a>
            </div>
          </div>
        </div>

        <!-- Company News Section -->
        <NewsSection
          v-if="companyDetails"
          :articles="newsArticles"
          :company-name="companyDetails.name"
          :symbol="companyDetails.ticker"
        />

      </div>
    </div>
  </div>
</template>
```