document.querySelectorAll(".toast").forEach(function (element) {
  bootstrap.Toast.getOrCreateInstance(element).show();
});

document.querySelectorAll("img").forEach(function (image) {
  function hideBrokenImage() {
    image.hidden = true;
    image.closest(".program-art, .gallery-cell")?.classList.add("image-missing");
  }
  if (image.complete && image.naturalWidth === 0) hideBrokenImage();
  else image.addEventListener("error", hideBrokenImage, { once: true });
});

document.querySelectorAll("form[data-save-form]").forEach(function (form) {
  form.addEventListener("submit", function () {
    const button = form.querySelector("[data-save-button]");
    if (!button || button.disabled) return;
    button.disabled = true;
    button.dataset.originalText = button.textContent;
    button.textContent = "Saving...";
  });
});