const formatter = new Intl.NumberFormat('en-IN', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const colors = [
        '#2563EB',
        '#16A085',
        '#E9A23B',
        '#8B5CF6',
        '#EC4899',
        '#64748B',
        '#F97316'
    ];

    const getElement = (id) => {
        const element = document.getElementById(id);

        if (!element) {
            throw new Error(`Element with ID '${id}' was not found.`);
        }

        return element;
    };

    fetch('/Expenses/GetChart')
        .then(response => {
            if (!response.ok) {
                throw new Error('Failed to load chart data.');
            }

            return response.json();
        })
        .then(data => {
            const expenses = Array.isArray(data) ? data : [];

            const breakdown = getElement('categoryBreakdown');
            const totalExpenses = getElement('totalExpenses');
            const totalCategories = getElement('totalCategories');
            const chartContainer = getElement('chartContainer');
            const chartEmpty = getElement('chartEmpty');

            breakdown.innerHTML = '';

            const labels = expenses.map(item => String(item.category));
            const amounts = expenses.map(item => Number(item.totalAmount));

            const total = amounts.reduce(
                (sum, amount) => sum + amount,
                0
            );

            totalExpenses.textContent = formatter.format(total);
            totalCategories.textContent = String(expenses.length);

            if (expenses.length === 0) {
                chartContainer.style.display = 'none';
                chartEmpty.style.display = 'flex';

                breakdown.textContent = 'No category data available yet.';
                return;
            }

            expenses.forEach((expense, index) => {
                const item = document.createElement('div');
                item.className = 'analysis-category-item';

                const categoryName = document.createElement('span');
                categoryName.className = 'analysis-category-name';

                const dot = document.createElement('span');
                dot.className = 'analysis-category-dot';
                dot.style.backgroundColor =
                    colors[index % colors.length];

                const name = document.createElement('span');
                name.textContent = String(expense.category);

                categoryName.appendChild(dot);
                categoryName.appendChild(name);

                const amount = document.createElement('span');
                amount.className = 'analysis-category-amount';
                amount.textContent =
                    '₹' + formatter.format(Number(expense.totalAmount));

                item.appendChild(categoryName);
                item.appendChild(amount);

                breakdown.appendChild(item);
            });

            const canvas = getElement('myChart');

            if (!(canvas instanceof HTMLCanvasElement)) {
                throw new Error('Chart canvas is invalid.');
            }

            const ChartConstructor = Reflect.get(window, 'Chart');

            if (typeof ChartConstructor !== 'function') {
                throw new Error('Chart.js has not loaded.');
            }

            new ChartConstructor(canvas, {
                type: 'doughnut',

                data: {
                    labels: labels,
                    datasets: [{
                        data: amounts,
                        backgroundColor: labels.map(
                            (_, index) => colors[index % colors.length]
                        ),
                        borderWidth: 3,
                        borderColor: '#ffffff',
                        hoverOffset: 6
                    }]
                },

                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    cutout: '65%',

                    plugins: {
                        legend: {
                            display: false
                        },

                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    return context.label + ': ₹' +
                                        formatter.format(Number(context.raw));
                                }
                            }
                        }
                    }
                }
            });
        })
        .catch(error => {
            console.error('Chart error:', error);

            const chartError = document.getElementById('chartError');
            const breakdown = document.getElementById('categoryBreakdown');

            if (chartError) {
                chartError.style.display = 'flex';
            }

            if (breakdown) {
                breakdown.textContent =
                    'Unable to load category data.';
            }
        });

