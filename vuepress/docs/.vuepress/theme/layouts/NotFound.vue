<template>
    <div class="notfound-container" v-if="state.display">
        <img src="/logomark.svg" />       

        <h1>404 Not Found</h1>

        <p>Sorry, this page doesn't exist. If you followed a link in the documentation to get here, feel free to update that file in GitHub.</p>

        <div style="margin-bottom: 10px"><RouterLink to="/">Developer Portal →</RouterLink></div>
    </div>
</template>

<script setup>
import { onBeforeMount, reactive } from 'vue'

const state = reactive({ display: false })

onBeforeMount(() => {
    if (window.document.referrer !== window.document.location.href){
        // Attempt to retrieve from server instead of vue route
        // This is required because relative links are handled as vue routes by vuepress - however they may point to another sub-site on the same domain.
        window.document.location = window.document.location.href;
        state.display = false;
    } else {
        state.display = true;
    }
})


</script>

