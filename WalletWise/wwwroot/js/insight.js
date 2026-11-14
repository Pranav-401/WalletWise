
    document.getElementById("generateInsightsBtn").addEventListener("click", async function () {
    const button = this;
    const loading = document.getElementById("insightsLoading");
    const error = document.getElementById("insightsError");
    const content = document.getElementById("insightsContent");

    button.disabled = true;
    button.textContent = "Generating...";
    loading.style.display = "block";
    error.style.display = "none";
    content.style.display = "none";

    try {
            const response = await fetch('/Analysis/GetInsights');

    const data = await response.json();

    if (!response.ok) {
                throw new Error(data.error || "Unable to generate insights.");
            }

    document.getElementById("insightsSummary").textContent = data.summary || "No summary available.";

    const observations = document.getElementById("insightsObservations");
    observations.replaceChildren();

            (data.observations || []).forEach(item => {
                const li = document.createElement("li");
    li.textContent = item;
    observations.appendChild(li);
            });

    const improvements = document.getElementById("insightsImprovements");
    improvements.replaceChildren();

            (data.improvements || []).forEach(item => {
                const div = document.createElement("div");
                div.className = "ai-insight-improvement";

    const title = document.createElement("h6");
    title.textContent = item.title;
    div.appendChild(title);

    const steps = document.createElement("ol");

                (item.steps || []).forEach(step => {
                    const li = document.createElement("li");
    li.textContent = step;
    steps.appendChild(li);
                });

    div.appendChild(steps);
    improvements.appendChild(div);
            });

    const positiveHabits = document.getElementById("insightsPositiveHabits");
    positiveHabits.replaceChildren();

            (data.positiveHabits || []).forEach(item => {
                const li = document.createElement("li");
    li.textContent = item;
    positiveHabits.appendChild(li);
            });

    content.style.display = "block";
        }
    catch (err) {
        document.getElementById("insightsErrorText").textContent =
            err.message || "Something went wrong. Please try again.";
    error.style.display = "block";
        }
    finally {
        loading.style.display = "none";
    button.disabled = false;
    button.textContent = "Generate Insights";
        }
    });
