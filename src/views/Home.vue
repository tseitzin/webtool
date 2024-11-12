<script setup lang="ts">
import { ref } from 'vue'

const searchQuery = ref('')
const selectedBook = ref('Genesis')
const selectedChapter = ref('1')
const selectedVerse = ref('1')

const bibleBooks = [
  'Genesis', 'Exodus', 'Leviticus', 'Numbers', 'Deuteronomy',
  'Joshua', 'Judges', 'Ruth', '1 Samuel', '2 Samuel',
  '1 Kings', '2 Kings', '1 Chronicles', '2 Chronicles', 'Ezra',
  'Nehemiah', 'Esther', 'Job', 'Psalms', 'Proverbs',
  'Ecclesiastes', 'Song of Solomon', 'Isaiah', 'Jeremiah', 'Lamentations',
  'Ezekiel', 'Daniel', 'Hosea', 'Joel', 'Amos',
  'Obadiah', 'Jonah', 'Micah', 'Nahum', 'Habakkuk',
  'Zephaniah', 'Haggai', 'Zechariah', 'Malachi',
  'Matthew', 'Mark', 'Luke', 'John', 'Acts',
  'Romans', '1 Corinthians', '2 Corinthians', 'Galatians', 'Ephesians',
  'Philippians', 'Colossians', '1 Thessalonians', '2 Thessalonians', '1 Timothy',
  '2 Timothy', 'Titus', 'Philemon', 'Hebrews', 'James',
  '1 Peter', '2 Peter', '1 John', '2 John', '3 John',
  'Jude', 'Revelation'
]

const featuredVerses = [
  {
    verse: "For God so loved the world, that he gave his only Son, that whoever believes in him should not perish but have eternal life.",
    reference: "John 3:16"
  },
  {
    verse: "I can do all things through Christ who strengthens me.",
    reference: "Philippians 4:13"
  },
  {
    verse: "Be strong and courageous. Do not be afraid; do not be discouraged, for the Lord your God will be with you wherever you go.",
    reference: "Joshua 1:9"
  }
]

</script>

<template>
  <div class="min-h-screen bg-gradient-to-b from-blue-50 to-white">
    <!-- Hero Section -->
    <div class="container mx-auto px-4 py-12">
      <div class="text-center mb-12">
        <h1 class="text-4xl md:text-5xl font-bold text-gray-900 mb-4">
          Welcome to the Bible Navigator
        </h1>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          Explore the wisdom of Scripture through reading and searching the Bible.
        </p>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          We hope that this helps you grow closer to the Word of God.
        </p>
        <p class="text-xl text-gray-600 max-w-2xl mx-auto">
          Start your spiritual journey today.
        </p>
      </div>

      <!-- Featured Verses -->
      <div class="max-w-4xl mx-auto">
        <h2 class="text-2xl font-semibold text-gray-800 mb-6 text-center">Featured Verses</h2>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div
            v-for="(feature, index) in featuredVerses"
            :key="index"
            class="bg-white p-6 rounded-lg shadow-lg hover:shadow-xl transition-shadow"
          >
            <p class="text-gray-700 mb-4 italic">
              "{{ feature.verse }}"
            </p>
            <p class="text-right text-blue-600 font-semibold">
              - {{ feature.reference }}
            </p>
          </div>
        </div>
      </div>

      <!-- Quick Links -->
      <div class="max-w-4xl mx-auto mt-12 text-center">
        <h2 class="text-2xl font-semibold text-gray-800 mb-6">Featured Books</h2>
        <div class="flex flex-wrap justify-center gap-4">
          <button
            v-for="book in ['Luke', 'Matthew', 'John', 'Mark']"
            :key="book"
            @click="selectedBook = book"
            class="px-6 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
          >
            {{ book }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
input[type="number"] {
  -moz-appearance: textfield;
}
input[type="number"]::-webkit-outer-spin-button,
input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
</style>