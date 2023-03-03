<template>
    <div class="notfound-container">
        <img src="/logomark.svg" />       

        <h1>404 Not Found</h1>

        <p>Sorry, this page doesn't exist. If you followed a link in the documentation to get here, feel free to update that file in GitHub.</p>

        <div style="margin-bottom: 10px"><RouterLink to="/">Developer Portal →</RouterLink></div>
    </div>
</template>

<script lang="ts">
if (window.document.referrer !== window.document.location.href){
    // Attempt to retrieve from server instead of vue route
    window.document.location = window.document.location.href;
}
</script>

