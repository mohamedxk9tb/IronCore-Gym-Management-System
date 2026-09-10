(function () {
  const canvas = document.getElementById("weightProgressChart");
  if (!canvas || !window.Chart) return;

  new Chart(canvas, {
    type: "line",
    data: {
      labels: ["Jun", "Jul", "Aug", "Sep", "Oct"],
      datasets: [{
        label: "Weight (kg)",
        data: [82, 81.2, 80.4, 79.2, 78.4],
        borderColor: "#FF6B00",
        backgroundColor: "rgba(255, 107, 0, .14)",
        borderWidth: 3,
        pointBackgroundColor: "#FF6B00",
        pointBorderColor: "#0D0D0D",
        pointBorderWidth: 3,
        pointRadius: 4,
        tension: .32,
        fill: true
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false }, tooltip: { backgroundColor: "#171717", borderColor: "#292929", borderWidth: 1, titleColor: "#F5F5F5", bodyColor: "#A3A3A3" } },
      scales: {
        x: { grid: { display: false }, border: { color: "#292929" }, ticks: { color: "#A3A3A3", font: { family: "Inter" } } },
        y: { min: 76, max: 83, grid: { color: "#292929" }, border: { display: false }, ticks: { color: "#A3A3A3", callback: (value) => `${value} kg`, font: { family: "Inter" } } }
      }
    }
  });
})();