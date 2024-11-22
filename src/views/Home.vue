<template>
  <div class="min-h-screen bg-gradient-to-b from-blue-50 to-white">
    <!-- Hero Section -->
    <div class="container mx-auto px-4 py-12">
      <div class="text-center mb-12">
        <h1 class="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
          Welcome to the Stock Navigator
        </h1>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          Join to explore various stocks, their price and any news related.
        </p>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          Feel free to join us and we hope that this helps you with building your financial portfolio.
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