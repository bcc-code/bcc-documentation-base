<template>
    <div>
        <Rating v-model="value"></Rating> {{ value }}

    </div>
</template>

<script type="ts" setup>


import { ref } from 'vue'

const value = ref(0)


</script>