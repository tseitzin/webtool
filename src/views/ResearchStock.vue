<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { polygonService } from '../services/polygonService'
import { formatNumber, formatCurrency } from '../utils/formatters'
import type { CompanyDetails, NewsArticle, StockData, RelatedCompany, HistoricalDataPoint } from '../types/polygon'
import StockDetailsCard from '../components/StockDetailsCard.vue'
import RelatedCompanies from '../components/RelatedCompanies.vue'
import NewsSection from '../components/NewsSection.vue'
import { stockService } from '../services/stockService'
import { useStockRemoval } from '../composables/useStockRemoval'
import type { TimeRange } from '../types/polygon'
import StockPriceChart from '../components/StockPriceChart.vue'
import AddToPortfolioModal from '../components/AddToPortfolioModal.vue'
import RemoveFromPortfolioModal from '../components/RemoveFromPortfolioModal.vue'
import { portfolioService } from '../services/portfolioService'
import type { UserOwnedStock } from '../types/portfolio'
import { showErrorToast } from '../utils/toast'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const companyDetails = ref<CompanyDetails | null>(null)
const newsArticles = ref<NewsArticle[]>([])
const stockData = ref<StockData | null>(null)
const relatedCompanies = ref<RelatedCompany[]>([])
const loading = ref(true)
const error = ref('')
const isSaved = ref(false)
const { showRemoveModal, stockToRemove, confirmRemoval, cancelRemoval } = useStockRemoval()
const historicalData = ref<HistoricalDataPoint[]>([])
const selectedTimeRange = ref<TimeRange>('1M')
const showPortfolioModal = ref(false)
const showRemovePortfolioModal = ref(false)
const selectedStockForPortfolio = ref<StockData | null>(null)
const ownedStocks = ref<UserOwnedStock[]>([])

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
    const [details, news, stock, related, historical, owned] = await Promise.all([
      polygonService.getCompanyDetails(symbol),
      polygonService.getCompanyNews(symbol),
      polygonService.getStockSnapshot(symbol),
      polygonService.getRelatedCompanies(symbol),
      polygonService.getHistoricalData(symbol, selectedTimeRange.value),
      portfolioService.getPortfolio()
    ])

    companyDetails.value = details
    newsArticles.value = news
    stockData.value = stock
    relatedCompanies.value = related
    historicalData.value = historical
    ownedStocks.value = owned

    // Check if stock is already saved
    const savedStocks = await stockService.getSavedStocks()
    isSaved.value = savedStocks.some(s => s.symbol === symbol)

  } catch (e) {
    error.value = 'Failed to load company information'
    console.error(e)
  } finally {
    loading.value = false
  }
})

const handleRemoveStock = (symbol: string) => {
  // Check if stock is in portfolio
  const isInPortfolio = ownedStocks.value.some(stock => stock.symbol === symbol)
  
  if (isInPortfolio) {
    showErrorToast(`Cannot remove ${symbol} from watchlist while it is in your portfolio`, 3000)
    return
  }
  
  confirmRemoval(symbol)
}

const handleConfirmRemoval = async () => {
  if (stockToRemove.value) {
    await toggleSaveStock()
    cancelRemoval()
  }
}

const handleTimeRangeChange = async (range: string) => {
  try {
    if (companyDetails.value) {
      const data = await polygonService.getHistoricalData(companyDetails.value.ticker, range)
      historicalData.value = data
    }
  } catch (error) {
    console.error('Failed to fetch historical data:', error)
  }
}

const toggleSaveStock = async () => {
  if (!stockData.value) return
  
  try {
    if (isSaved.value) {
      await stockService.removeSavedStock(stockData.value.symbol)
    } else {
      await stockService.saveStock(stockData.value)
    }
    isSaved.value = !isSaved.value
  } catch (e) {
    error.value = 'Failed to update saved stocks'
    console.error(e)
  }
}

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

const openPortfolioModal = (stock: StockData) => {
  selectedStockForPortfolio.value = stock
  showPortfolioModal.value = true
}

const closePortfolioModal = () => {
  showPortfolioModal.value = false
  selectedStockForPortfolio.value = null
  location.reload()
}

const openRemovePortfolioModal = (stock: StockData) => {
  selectedStockForPortfolio.value = stock
  showRemovePortfolioModal.value = true
}

const closeRemovePortfolioModal = () => {
  showRemovePortfolioModal.value = false
  selectedStockForPortfolio.value = null
}

const getOwnershipInfo = (symbol: string) => {
  const owned = ownedStocks.value.find(stock => stock.symbol === symbol)
  if (!owned) return null
  
  return {
    shares: owned.quantity,
    value: owned.quantity * owned.averagePurchasePrice
  }
}

const handlePortfolioSuccess = async () => {
  // Refresh data after successful portfolio update
  const owned = await portfolioService.getPortfolio()
  ownedStocks.value = owned
}
</script>

<template>
  <div class="min-h-screen bg-gray-100">
    <div class="container mx-auto px-4 py-4 sm:py-8">
      <!-- Loading State -->
      <div v-if="loading" class="flex justify-center items-center py-8">
        <div class="animate-spin rounded-full h-12 w-12 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
        {{ error }}
      </div>

      <!-- Company Details -->
      <div v-else-if="companyDetails" class="space-y-4 sm:space-y-6">
        <!-- Header Section -->
        <div class="bg-white rounded-lg shadow-md p-4 sm:p-6">
          <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div class="w-full sm:w-auto">
              <h1 class="text-lg sm:text-2xl font-semibold text-gray-900 break-words">
                Company Name: <p>{{ companyDetails.name }}</p>
              </h1>
              <p class="text-lg sm:text-xl font-semibold text-gray-800">
                Stock Symbol: {{ companyDetails.ticker }}
              </p>
            </div>
            <div class="flex flex-col sm:flex-row gap-2 w-full sm:w-auto">
              <button
                @click="navigateToSearch"
                class="px-4 py-2 bg-blue-600 text-xs text-white rounded-lg hover:bg-blue-800 transition-colors"
                >
                Return to Search
              </button>
              <button
                v-if="stockData"
                @click="openPortfolioModal(stockData)"
                class="px-4 py-2 bg-green-600 text-xs text-white rounded-lg hover:bg-green-800 transition-colors"
                >
                Buy Stock
              </button>
              <button
                v-if="stockData && getOwnershipInfo(stockData.symbol)"
                @click="openRemovePortfolioModal(stockData)"
                class="px-4 py-2 bg-red-600 text-xs text-white rounded-lg hover:bg-red-800 transition-colors"
                >
                Sell Stock
              </button>
              <button
                @click="isSaved ? handleRemoveStock(companyDetails.ticker) : toggleSaveStock()"
                class="px-4 py-2 bg-gray-600 text-xs text-white rounded-lg hover:bg-gray-800 transition-colors"
                :class="isSaved ? 'bg-gray-600 text-xs text-white rounded-lg hover:bg-gray-800' : 'bg-green-600 text-white hover:bg-green-700'"
              >
                {{ isSaved ? 'Remove from Watchlist' : 'Add to Watchlist' }}
              </button>
              
            </div>
          </div>

          <div v-if="stockData">
            <div 
              v-if="getOwnershipInfo(stockData.symbol)"
              class="bg-blue-50 p-4 rounded-lg mt-2"
            >
              <div class="flex flex-col sm:flex-row gap-4">
                <div>
                  <span class="font-medium text-blue-800">Number in Portfolio:</span>
                  <span class="ml-2 text-blue-700">
                    {{ formatNumber(getOwnershipInfo(stockData.symbol)?.shares || 0) }} shares
                  </span>
                </div>
                <div>
                  <span class="font-medium text-blue-800">Total Value:</span>
                  <span class="ml-2 text-blue-700">
                    {{ formatCurrency((getOwnershipInfo(stockData.symbol)?.value || 0)) }}
                  </span>
                </div>
              </div>
            </div>
            <div 
              v-else 
              class="bg-gray-50 p-4 rounded-lg mt-2"
            >
              <p class="font-semibold text-gray-600">Not currently in portfolio</p>
            </div>
          </div>
        </div>

        <!-- Stock Details Section -->
        <StockDetailsCard
          v-if="stockData"
          :stock="stockData"
        />

        <!-- Stock Price Chart -->
        <StockPriceChart
          v-if="historicalData.length > 0"
          :data="historicalData"
          :symbol="companyDetails.ticker"
          @time-range-changed="handleTimeRangeChange"
          class="mb-6"
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
        <div class="bg-white rounded-lg shadow-md p-4 sm:p-6">
          <h2 class="text-xl font-semibold mb-4">Key Statistics</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Market Cap</p>
              <p class="text-md font-semibold">{{ formatMarketCap(companyDetails.market_cap) }}</p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Employees</p>
              <p class="text-md font-semibold">{{ formatNumber(companyDetails.total_employees) }}</p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Listed Since</p>
              <p class="text-md font-semibold">{{ formatDate(companyDetails.list_date) }}</p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Exchange</p>
              <p class="text-md font-semibold">{{ companyDetails.primary_exchange }}</p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Industry</p>
              <p class="text-sm font-semibold">{{ companyDetails.sic_description }}</p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Outstanding Shares</p>
              <p class="text-md font-semibold">{{ formatNumber(companyDetails.weighted_shares_outstanding) }}</p>
            </div>
          </div>
        </div>

        <!-- Company Description -->
        <div class="bg-white rounded-lg shadow-md p-4 sm:p-6">
          <h2 class="text-xl font-semibold mb-4">About {{ companyDetails.name }}</h2>
          <p class="text-gray-700 leading-relaxed text-sm sm:text-base mb-6">
            {{ companyDetails.description }}
          </p>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Headquarters</p>
              <p class="text-sm font-semibold">{{ companyDetails.address.address1 }}</p>
              <p class="text-sm font-semibold">
                {{ companyDetails.address.city }}, {{ companyDetails.address.state }} 
                {{ companyDetails.address.postal_code }}
              </p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Phone</p>
              <p class="text-sm font-semibold">{{ companyDetails.phone_number }}</p>
            </div>
            <div class="bg-gray-50 p-3 rounded-lg">
              <p class="text-sm text-gray-500">Website</p>
              <a 
                :href="companyDetails.homepage_url"
                target="_blank"
                rel="noopener noreferrer"
                class="text-sm font-semibold text-indigo-600 hover:text-indigo-800 break-words"
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

        <!-- Modals -->
        <ConfirmationModal
          :is-open="showRemoveModal"
          title="Remove Stock"
          message="Are you sure you want to remove this stock from your watchlist?"
          @confirm="handleConfirmRemoval"
          @cancel="cancelRemoval"
        />

        <AddToPortfolioModal
          v-if="showPortfolioModal && selectedStockForPortfolio"
          :is-open="showPortfolioModal"
          :symbol="selectedStockForPortfolio.symbol"
          :price="selectedStockForPortfolio.price"
          @close="closePortfolioModal"
          @success="handlePortfolioSuccess"
        />

        <RemoveFromPortfolioModal
          v-if="selectedStockForPortfolio"
          :is-open="showRemovePortfolioModal"
          :symbol="selectedStockForPortfolio.symbol"
          :price="selectedStockForPortfolio.price"
          @close="closeRemovePortfolioModal"
          @success="handlePortfolioSuccess"
        />
      </div>
    </div>
  </div>
</template>