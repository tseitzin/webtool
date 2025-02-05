<template>
  <div class="min-h-screen bg-gradient-to-b from-blue-50 to-white">
    <!-- Hero Section -->
    <div class="container mx-auto px-4 py-12">
      <div class="text-center mb-12">
        <h1 class="text-4xl md:text-5xl font-bold text-gray-900 mb-6">
          Welcome to the Financial Navigator
        </h1>
        <p class="text-xl text-gray-700 max-w-3xl mx-auto mb-8">
          Your comprehensive platform for tracking stocks and cryptocurrencies, managing portfolios, 
          and making informed investment decisions.
        </p>
      </div>

      <!-- Key Features Section -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8 mb-16 max-w-7xl mx-auto">
        <div class="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
          <h3 class="text-xl font-bold text-indigo-600 mb-4">Real-Time Market Data</h3>
          <p class="text-gray-600">
            Access live stock prices, market trends, and comprehensive financial data to stay informed 
            about your investments.
          </p>
        </div>
        <div class="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
          <h3 class="text-xl font-bold text-indigo-600 mb-4">Portfolio Management</h3>
          <p class="text-gray-600">
            Track your investments, monitor performance, and manage your portfolio with advanced tools 
            and analytics.
          </p>
        </div>
        <div class="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition-shadow">
          <h3 class="text-xl font-bold text-indigo-600 mb-4">Cryptocurrency Tracking</h3>
          <p class="text-gray-600">
            Stay updated with real-time cryptocurrency prices, market trends, and portfolio tracking 
            capabilities.
          </p>
        </div>
      </div>

      <!-- How It Works Section -->
      <div class="bg-white rounded-xl shadow-lg p-8 mb-16 max-w-7xl mx-auto">
        <h2 class="text-3xl font-bold text-center mb-8">How It Works</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div class="text-center">
            <div class="bg-indigo-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-4">
              <span class="text-xl font-bold text-indigo-600">1</span>
            </div>
            <h3 class="font-semibold mb-2">Create Account</h3>
            <p class="text-gray-600">Sign up for free and set up your personalized dashboard</p>
          </div>
          <div class="text-center">
            <div class="bg-indigo-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-4">
              <span class="text-xl font-bold text-indigo-600">2</span>
            </div>
            <h3 class="font-semibold mb-2">Build Watchlist</h3>
            <p class="text-gray-600">Add stocks and cryptocurrencies to your watchlist</p>
          </div>
          <div class="text-center">
            <div class="bg-indigo-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-4">
              <span class="text-xl font-bold text-indigo-600">3</span>
            </div>
            <h3 class="font-semibold mb-2">Track Portfolio</h3>
            <p class="text-gray-600">Monitor your investments and track performance</p>
          </div>
          <div class="text-center">
            <div class="bg-indigo-100 w-12 h-12 rounded-full flex items-center justify-center mx-auto mb-4">
              <span class="text-xl font-bold text-indigo-600">4</span>
            </div>
            <h3 class="font-semibold mb-2">Make Decisions</h3>
            <p class="text-gray-600">Use insights and data to make informed investment choices</p>
          </div>
        </div>
      </div>

      <!-- Benefits Section -->
      <div class="max-w-7xl mx-auto mb-16">
        <h2 class="text-3xl font-bold text-center mb-8">Benefits of Using Our Platform</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div class="bg-white p-6 rounded-xl shadow-lg">
            <h3 class="text-xl font-bold text-indigo-600 mb-4">For New Investors</h3>
            <ul class="space-y-3">
              <li class="flex items-center">
                <span class="text-green-500 mr-2">✓</span>
                Easy-to-use interface for tracking investments
              </li>
              <li class="flex items-center">
                <span class="text-green-500 mr-2">✓</span>
                Educational resources and market insights
              </li>
              <li class="flex items-center">
                <span class="text-green-500 mr-2">✓</span>
                Basic portfolio management tools
              </li>
            </ul>
          </div>
          <div class="bg-white p-6 rounded-xl shadow-lg">
            <h3 class="text-xl font-bold text-indigo-600 mb-4">For Experienced Traders</h3>
            <ul class="space-y-3">
              <li class="flex items-center">
                <span class="text-green-500 mr-2">✓</span>
                Advanced analytics and market data
              </li>
              <li class="flex items-center">
                <span class="text-green-500 mr-2">✓</span>
                Comprehensive portfolio tracking
              </li>
              <li class="flex items-center">
                <span class="text-green-500 mr-2">✓</span>
                Real-time market updates and alerts
              </li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Get Started Section -->
      <div class="text-center max-w-3xl mx-auto">
        <h2 class="text-3xl font-bold mb-6">Ready to Start Your Financial Journey?</h2>
        <p class="text-gray-600 mb-8">
          Join thousands of investors who are already using our platform to track their investments 
          and make better financial decisions.
        </p>
        <div class="tiles-grid">
  <router-link 
    v-for="tile in navigationTiles" 
    :key="tile.route"
    :to="tile.route"
    class="tile"
  >
  <img :src="getIcon(tile.icon)" :alt="tile.title" />
    <h2 class="tile-title">{{ tile.title }}</h2>
    <p class="tile-description">{{ tile.description }}</p>
  </router-link>
</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">

interface NavTile {
    title: string
    description: string
    route: string
    icon: string
  }
  
  const navigationTiles: NavTile[] = [
    {
      title: 'Login to your account',
      description: 'Login and start trading!',
      route: '/login',
      icon: '/market.svg'
    },
    {
      title: 'Register as new user',
      description: 'Sign-up and start your financial journey',
      route: '/register',
      icon: '/register.svg'
    }
  ]

  function getIcon(iconPath: string) {
  return new URL(iconPath, import.meta.url).href
}

</script>


<style scoped>
input[type="number"] {
  -moz-appearance: textfield;
}
input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.tiles-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 2rem;
    padding: 1rem;
    max-width: 800px;
    margin: 0 auto;
  }
  
  .tile {
    background: white;
    border-radius: 12px;
    padding: 2rem;
    text-decoration: none;
    color: inherit;
    transition: transform 0.2s, box-shadow 0.2s;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    display: flex;
    flex-direction: column;
    align-items: center;
    cursor: pointer;
  }
  
  .tile:hover {
    transform: translateY(-4px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  }
  
  .tile-icon {
    font-size: 2.5rem;
    margin-bottom: 1rem;
  }
  
  .tile-title {
    font-size: 1.5rem;
    margin-bottom: 0.5rem;
    color: #333;
  }
  
  .tile-description {
    color: #666;
    font-size: 0.9rem;
    line-height: 1.4;
  }
  
  @media (max-width: 768px) {
    .tiles-grid {
      grid-template-columns: 1fr;
      padding: 0.5rem;
    }
    
    .tile {
      padding: 1.5rem;
    }
  }
</style>