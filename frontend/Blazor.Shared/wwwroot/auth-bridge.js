window.authBridge = {
    storageKey: 'registro-auth-mf',
    shellStorageKey: 'registro-auth',
    _listenerRegistered: false,
    normalizeSession: function (payload) {
        if (!payload) return null;
        var sessionId = payload.sessionId || payload.SessionId || '';
        return {
            accessToken: payload.accessToken || payload.AccessToken || '',
            refreshToken: payload.refreshToken || payload.RefreshToken || '',
            sessionId: sessionId,
            expiresInSeconds: payload.expiresInSeconds || payload.ExpiresInSeconds || 0,
            rol: payload.rol || payload.Rol || '',
            estudianteId: payload.estudianteId != null ? payload.estudianteId : payload.EstudianteId,
            nombre: payload.nombre || payload.Nombre || null
        };
    },
    readShellStorage: function () {
        try {
            var raw = localStorage.getItem(this.shellStorageKey);
            if (raw) return this.normalizeSession(JSON.parse(raw));
        } catch (e) { }
        return null;
    },
    readFromParent: function () {
        try {
            if (window.parent !== window) {
                var raw = window.parent.localStorage.getItem(this.shellStorageKey);
                if (raw) return this.normalizeSession(JSON.parse(raw));
            }
        } catch (e) { }
        return null;
    },
    readAnyShellSession: function () {
        return this.readHashAuth() || this.readShellStorage() || this.readFromParent();
    },
    readHashAuth: function () {
        var hash = window.location.hash || '';
        if (hash.indexOf('#auth=') !== 0) return null;
        try {
            return this.normalizeSession(JSON.parse(decodeURIComponent(hash.substring(6))));
        } catch (e) {
            return null;
        }
    },
    bootstrap: function () {
        var self = this;
        function tryLoad() {
            var session = self.readAnyShellSession() || self.getStoredSession();
            if (session && session.accessToken) {
                sessionStorage.setItem(self.storageKey, JSON.stringify(session));
                return session;
            }
            return null;
        }
        if (tryLoad()) return;
        var attempts = 0;
        var timer = setInterval(function () {
            if (tryLoad() || ++attempts >= 30) clearInterval(timer);
        }, 200);
    },
    storeSession: function (payload) {
        var session = this.normalizeSession(payload);
        if (!session || !session.accessToken) return null;
        sessionStorage.setItem(this.storageKey, JSON.stringify(session));
        return session;
    },
    getStoredSession: function () {
        var raw = sessionStorage.getItem(this.storageKey);
        return raw ? JSON.parse(raw) : null;
    },
    getStoredSessionJson: function () {
        return sessionStorage.getItem(this.storageKey);
    },
    getShellSessionJson: function () {
        var session = this.readAnyShellSession();
        return session ? JSON.stringify(session) : null;
    },
    clearStoredSession: function () {
        sessionStorage.removeItem(this.storageKey);
    },
    applySession: function (dotNetRef, session) {
        if (!session || !session.accessToken) return;
        this.storeSession(session);
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('ReceiveAuthJson', JSON.stringify(session)).catch(function () { });
        }
    },
    init: function (dotNetRef) {
        this._dotNetRef = dotNetRef;
        var session = this.getStoredSession() || this.readAnyShellSession();
        if (session) this.applySession(dotNetRef, session);

        if (!this._listenerRegistered) {
            this._listenerRegistered = true;
            var self = this;
            window.addEventListener('message', function (e) {
                if (e.data && e.data.type === 'registro-auth-init' && e.data.payload) {
                    self.applySession(self._dotNetRef, e.data.payload);
                }
            });
        }

        var self = this;
        var attempts = 0;
        var timer = setInterval(function () {
            var s = self.readAnyShellSession();
            if (s) {
                self.applySession(dotNetRef, s);
                clearInterval(timer);
            } else if (++attempts >= 50) {
                clearInterval(timer);
            }
        }, 200);
    },
    notifyLogin: function (payload) {
        if (window.parent !== window) {
            window.parent.postMessage({ type: 'registro-auth', payload: payload }, '*');
        }
    },
    notifyLogout: function () {
        this.clearStoredSession();
        if (window.parent !== window) {
            window.parent.postMessage({ type: 'registro-auth-logout' }, '*');
        }
    }
};

window.shellAuth = {
    storageKey: 'registro-auth',
    _relayInitialized: false,
    _relayTimer: null,
    getAccessToken: function (data) {
        return data.accessToken || data.AccessToken || null;
    },
    getExpiresIn: function (data) {
        return data.expiresInSeconds || data.ExpiresInSeconds || 0;
    },
    isExpired: function (data) {
        var savedAt = data.savedAt || 0;
        var expiresIn = this.getExpiresIn(data);
        if (!savedAt) return true;
        if (!expiresIn) return false;
        return (Date.now() - savedAt) > expiresIn * 1000;
    },
    routeGuard: function () {
        var path = location.pathname.replace(/\/+$/, '') || '/';
        var isLogin = path === '/' || path === '/login';
        var isProtected = path === '/home' || path === '/estudiantes' || path === '/directorio-estudiantes'
            || path === '/inscripciones' || path === '/materias' || path === '/profesores';
        var session = this.get();
        if (!session && isProtected) {
            location.replace('/');
            return;
        }
        if (session && isLogin) {
            location.replace('/home');
        }
    },
    initRelay: function () {
        if (this._relayInitialized) return;
        this._relayInitialized = true;
        var self = this;
        window.addEventListener('message', function (e) {
            if (!e.data) return;
            if (e.data.type === 'registro-auth' && e.data.payload) {
                self.set(e.data.payload);
            }
            if (e.data.type === 'registro-auth-logout') {
                self.clear();
            }
        });
    },
    get: function () {
        var raw = localStorage.getItem(this.storageKey);
        if (!raw) return null;
        var data = JSON.parse(raw);
        if (!this.getAccessToken(data)) {
            localStorage.removeItem(this.storageKey);
            return null;
        }
        if (this.isExpired(data)) {
            localStorage.removeItem(this.storageKey);
            return null;
        }
        return data;
    },
    set: function (payload) {
        var data = Object.assign({}, payload, { savedAt: Date.now() });
        localStorage.setItem(this.storageKey, JSON.stringify(data));
    },
    clear: function () {
        localStorage.removeItem(this.storageKey);
    },
    onLogoutMessage: function (dotNetRef) {
        window.addEventListener('message', function (e) {
            if (e.data && e.data.type === 'registro-auth-logout') {
                window.shellAuth.clear();
                dotNetRef.invokeMethodAsync('OnLoggedOut');
            }
        });
    },
    relayToFrame: function (frameEl) {
        if (!frameEl) return;
        this.relayAllFrames();
    },
    relayAllFrames: function () {
        var session = this.get();
        if (!session) return;
        if (this._relayTimer) return;
        var frames = Array.from(document.querySelectorAll('iframe.mf-frame'));
        if (!frames.length) return;
        var self = this;
        function send() {
            frames.forEach(function (frame) {
                try {
                    if (frame.contentWindow) {
                        frame.contentWindow.postMessage(
                            { type: 'registro-auth-init', payload: session }, '*');
                    }
                } catch (e) { }
            });
        }
        send();
        var attempts = 0;
        self._relayTimer = setInterval(function () {
            send();
            if (++attempts >= 30) {
                clearInterval(self._relayTimer);
                self._relayTimer = null;
            }
        }, 200);
    }
};
