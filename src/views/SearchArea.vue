<template>
    <div class="home">
      <p class="title">Welcome to the Stock search area</p>
      
      
    </div>
  </template>
  
  <script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { onMounted } from 'vue';

  const router = useRouter()
  const auth = useAuthStore()

  onMounted(async () => {
    if (!auth.user) {
      router.push('/access-denied')
    }
  })
  
  
  </script>
  
  <style scoped>
  .title {
    font-size: 1.5rem; 
    font-weight: bold;
  }
  .home {
    padding: 2rem;
    text-align: center;
    min-height: 100vh;
    background-color: rgb(248 250 252);
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