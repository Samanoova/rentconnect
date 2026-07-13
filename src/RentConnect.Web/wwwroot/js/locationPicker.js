window.locationPicker = {
    maps: {},

    init: function (elementId, dotnetRef, lat, lng) {
        const el = document.getElementById(elementId);
        if (!el) return;

        // لو الخريطة موجودة أصلاً بهذا العنصر (مثلاً بعد re-render)، لا تنشئها من جديد
        if (this.maps[elementId]) return;

        const map = L.map(elementId).setView([lat, lng], 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        const marker = L.marker([lat, lng]).addTo(map);

        map.on('click', function (e) {
            marker.setLatLng(e.latlng);
            dotnetRef.invokeMethodAsync('OnMapClicked', e.latlng.lat, e.latlng.lng);
        });

        this.maps[elementId] = { map: map, marker: marker };

        // Leaflet بيحتاج إعادة حساب الأبعاد لو الحاوية كانت مخفية وقت الإنشاء (مثلاً داخل Modal)
        setTimeout(function () { map.invalidateSize(); }, 200);
    },

    // خريطة عرض فقط (بدون تفاعل تحديد موقع) - تُستخدم بصفحة تفاصيل الإعلان لإظهار موقع العقار بعد كشف الرقم
    initReadOnly: function (elementId, lat, lng) {
        const el = document.getElementById(elementId);
        if (!el) return;
        if (this.maps[elementId]) return;

        const map = L.map(elementId, { scrollWheelZoom: false }).setView([lat, lng], 15);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        L.marker([lat, lng]).addTo(map);

        this.maps[elementId] = { map: map };
        setTimeout(function () { map.invalidateSize(); }, 200);
    },

    // ينقل خريطة تحديد الموقع (القابلة للتعديل) لمكان معيّن - تُستخدم لما المستخدم يختار نتيجة بحث
    setView: function (elementId, lat, lng) {
        const entry = this.maps[elementId];
        if (!entry) {
            console.warn('locationPicker.setView: map not found for', elementId);
            return;
        }

        // إعادة حساب أبعاد الخريطة قبل النقل، تحسباً لو كانت الحاوية اختلف حجمها منذ إنشائها
        entry.map.invalidateSize();
        entry.map.setView([lat, lng], 16);

        if (entry.marker) {
            entry.marker.setLatLng([lat, lng]);
        } else {
            entry.marker = L.marker([lat, lng]).addTo(entry.map);
        }
    },

    locate: function (elementId, dotnetRef) {
        if (!navigator.geolocation) {
            dotnetRef.invokeMethodAsync('OnLocationError', 'المتصفح لا يدعم تحديد الموقع.');
            return;
        }

        navigator.geolocation.getCurrentPosition(
            function (pos) {
                const lat = pos.coords.latitude;
                const lng = pos.coords.longitude;

                const entry = window.locationPicker.maps[elementId];
                if (entry) {
                    entry.marker.setLatLng([lat, lng]);
                    entry.map.setView([lat, lng], 15);
                }

                dotnetRef.invokeMethodAsync('OnCurrentLocationDetected', lat, lng);
            },
            function (err) {
                dotnetRef.invokeMethodAsync('OnLocationError', 'تعذّر الحصول على الموقع الحالي: ' + err.message);
            },
            { enableHighAccuracy: true, timeout: 10000 }
        );
    }
};
