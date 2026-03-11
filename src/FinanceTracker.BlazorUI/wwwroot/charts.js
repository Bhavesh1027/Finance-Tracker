window.reportsCharts = {
    downloadElementAsPng: async function (elementId, fileName) {
        const elem = document.getElementById(elementId);
        if (!elem) {
            return;
        }

        const rect = elem.getBoundingClientRect();
        const svg = elem.querySelector("svg");
        if (!svg) {
            return;
        }

        const serializer = new XMLSerializer();
        const svgString = serializer.serializeToString(svg);
        const svgBlob = new Blob([svgString], { type: "image/svg+xml;charset=utf-8" });
        const url = URL.createObjectURL(svgBlob);

        const img = new Image();
        img.onload = function () {
            const canvas = document.createElement("canvas");
            canvas.width = rect.width;
            canvas.height = rect.height;
            const ctx = canvas.getContext("2d");
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
            URL.revokeObjectURL(url);

            canvas.toBlob(function (blob) {
                const link = document.createElement("a");
                link.href = URL.createObjectURL(blob);
                link.download = fileName || "chart.png";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            });
        };
        img.src = url;
    }
};

