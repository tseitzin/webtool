<template>
    <div class="home">
      <p class="title">Welcome to the Bible search area</p>
      <p><strong>Select a tile to start searching</strong></p>
      
      <div class="tiles-grid">
        <router-link 
          v-for="tile in navigationTiles" 
          :key="tile.route"
          :to="tile.route"
          class="tile"
        >
          <div class="tile-icon">{{ tile.icon }}</div>
          <h2 class="tile-title">{{ tile.title }}</h2>
          <p class="tile-description">{{ tile.description }}</p>
        </router-link>
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
      title: 'Search for a Verse',
      description: 'Find a specific verse in the Bible',
      route: '/verse',
      icon: '📜'
    },
    {
      title: 'Search for a Chapter',
      description: 'Find an entire chapter in the Bible',
      route: '/about',
      icon: '📝'
    },
    {
      title: 'Search for a Book',
      description: 'Find an entire book in the Bible',
      route: '/about',
      icon: '📖'
    }
  ]
  </script>
  
  <style scoped>
  .title {
    font-size: 1.5rem; 
    font-weight: bold;
  }
  .home {
    padding: 2rem;
    text-align: center;
  }
  
  .subtitle {
    color: #666;
    margin-bottom: 2rem;
  }
  
  .tiles-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 2rem;
    padding: 1rem;
    max-width: 1200px;
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