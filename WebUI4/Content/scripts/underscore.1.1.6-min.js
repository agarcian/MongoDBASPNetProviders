//     Underscore.js 1.1.6
//     (c) 2011 Jeremy Ashkenas, DocumentCloud Inc.
//     Underscore is freely distributable under the MIT license.
//     Portions of Underscore are inspired or borrowed from Prototype,
//     Oliver Steele's Functional, and John Resig's Micro-Templating.
//     For all details and documentation:
//     http://documentcloud.github.com/underscore

var h = null;
(function () {
    function E(a, c) { l.prototype[a] = function () { var a = f.call(arguments); F.call(a, this.a); return q(c.apply(b, a), this.b) } } function q(a, c) { return c ? b(a).n() : a } function l(a) { this.a = a } function u(a, c, b) { var e; return function () { function k() { e = h; a.apply(g, r) } var g = this, r = arguments; b && clearTimeout(e); if (b || !e) e = setTimeout(k, c) } } function b(a) { return new l(a) } var s = this, G = s.f, n = {}, j = Array.prototype, o = Object.prototype, f = j.slice, F = j.unshift, H = o.toString, m = o.hasOwnProperty, v = j.forEach, w = j.map, x = j.reduce,
y = j.reduceRight, z = j.filter, A = j.every, B = j.some, p = j.indexOf, C = j.lastIndexOf, o = Array.isArray, I = Object.keys, t = Function.prototype.bind; typeof module !== "undefined" && module.p ? (module.p = b, b.f = b) : s.f = b; b.VERSION = "1.1.6"; var i = b.N = b.forEach = function (a, c, d) { if (a != h) if (v && a.forEach === v) a.forEach(c, d); else if (b.u(a.length)) for (var e = 0, k = a.length; e < k; e++) { if (c.call(d, a[e], e, a) === n) break } else for (e in a) if (m.call(a, e) && c.call(d, a[e], e, a) === n) break }; b.map = function (a, c, b) {
    var e = []; if (a == h) return e; if (w && a.map === w) return a.map(c,
b); i(a, function (a, g, r) { e[e.length] = c.call(b, a, g, r) }); return e
}; b.reduce = b.P = b.S = function (a, c, d, e) { var k = d !== void 0; a == h && (a = []); if (x && a.reduce === x) return e && (c = b.bind(c, e)), k ? a.reduce(c, d) : a.reduce(c); i(a, function (a, b, f) { !k && b === 0 ? (d = a, k = !0) : d = c.call(e, d, a, b, f) }); if (!k) throw new TypeError("Reduce of empty array with no initial value"); return d }; b.reduceRight = b.Q = function (a, c, d, e) {
    a == h && (a = []); if (y && a.reduceRight === y) return e && (c = b.bind(c, e)), d !== void 0 ? a.reduceRight(c, d) : a.reduceRight(c); a = (b.isArray(a) ?
a.slice() : b.e(a)).reverse(); return b.reduce(a, c, d, e)
}; b.find = b.M = function (a, c, b) { var e; D(a, function (a, g, f) { if (c.call(b, a, g, f)) return e = a, !0 }); return e }; b.filter = b.select = function (a, c, b) { var e = []; if (a == h) return e; if (z && a.filter === z) return a.filter(c, b); i(a, function (a, g, f) { c.call(b, a, g, f) && (e[e.length] = a) }); return e }; b.ea = function (a, c, b) { var e = []; if (a == h) return e; i(a, function (a, g, f) { c.call(b, a, g, f) || (e[e.length] = a) }); return e }; b.every = b.all = function (a, c, b) {
    var e = !0; if (a == h) return e; if (A && a.every ===
A) return a.every(c, b); i(a, function (a, g, f) { if (!(e = e && c.call(b, a, g, f))) return n }); return e
}; var D = b.some = b.G = function (a, c, d) { c || (c = b.d); var e = !1; if (a == h) return e; if (B && a.some === B) return a.some(c, d); i(a, function (a, b, f) { if (e = c.call(d, a, b, f)) return n }); return e }; b.j = b.contains = function (a, c) { var b = !1; if (a == h) return b; if (p && a.indexOf === p) return a.indexOf(c) != -1; D(a, function (a) { if (b = a === c) return !0 }); return b }; b.U = function (a, c) {
    var d = f.call(arguments, 2); return b.map(a, function (a) {
        return (c.call ? c || a : a[c]).apply(a,
d)
    })
}; b.g = function (a, c) { return b.map(a, function (a) { return a[c] }) }; b.max = function (a, c, d) { if (!c && b.isArray(a)) return Math.max.apply(Math, a); var e = { c: -Infinity }; i(a, function (a, b, f) { b = c ? c.call(d, a, b, f) : a; b >= e.c && (e = { value: a, c: b }) }); return e.value }; b.min = function (a, c, d) { if (!c && b.isArray(a)) return Math.min.apply(Math, a); var e = { c: Infinity }; i(a, function (a, b, f) { b = c ? c.call(d, a, b, f) : a; b < e.c && (e = { value: a, c: b }) }); return e.value }; b.ga = function (a, c, d) {
    return b.g(b.map(a, function (a, b, f) {
        return { value: a, h: c.call(d,
a, b, f)
        }
    }).sort(function (a, b) { var c = a.h, d = b.h; return c < d ? -1 : c > d ? 1 : 0 }), "value")
}; b.A = function (a, c) { var d; d || (d = b.d); for (var e = 0, f = a.length; e < f; ) { var g = e + f >> 1; d(a[g]) < d(c) ? e = g + 1 : f = g } return e }; b.e = function (a) { return !a ? [] : a.e ? a.e() : b.isArray(a) ? a : b.s(a) ? f.call(a) : b.D(a) }; b.size = function (a) { return b.e(a).length }; b.O = b.R = function (a, b, d) { return b != h && !d ? f.call(a, 0, b) : a[0] }; b.fa = b.ha = function (a, b, d) { return f.call(a, b == h || d ? 1 : b) }; b.w = function (a) { return a[a.length - 1] }; b.compact = function (a) {
    return b.filter(a,
function (a) { return !!a })
}; b.q = function (a) { return b.reduce(a, function (a, d) { if (b.isArray(d)) return a.concat(b.q(d)); a[a.length] = d; return a }, []) }; b.na = function (a) { var c = f.call(arguments, 1); return b.filter(a, function (a) { return !b.j(c, a) }) }; b.C = b.unique = function (a, c) { return b.reduce(a, function (a, e, f) { if (0 == f || (c === !0 ? b.w(a) != e : !b.j(a, e))) a[a.length] = e; return a }, []) }; b.T = function (a) { var c = f.call(arguments, 1); return b.filter(b.C(a), function (a) { return b.every(c, function (c) { return b.indexOf(c, a) >= 0 }) }) }; b.pa =
function () { for (var a = f.call(arguments), c = b.max(b.g(a, "length")), d = Array(c), e = 0; e < c; e++) d[e] = b.g(a, "" + e); return d }; b.indexOf = function (a, c, d) { if (a == h) return -1; var e; if (d) return d = b.A(a, c), a[d] === c ? d : -1; if (p && a.indexOf === p) return a.indexOf(c); for (d = 0, e = a.length; d < e; d++) if (a[d] === c) return d; return -1 }; b.lastIndexOf = function (a, b) { if (a == h) return -1; if (C && a.lastIndexOf === C) return a.lastIndexOf(b); for (var d = a.length; d--; ) if (a[d] === b) return d; return -1 }; b.da = function (a, b, d) {
    arguments.length <= 1 && (b = a || 0, a = 0);
    for (var d = arguments[2] || 1, e = Math.max(Math.ceil((b - a) / d), 0), f = 0, g = Array(e); f < e; ) g[f++] = a, a += d; return g
}; b.bind = function (a, b) { if (a.bind === t && t) return t.apply(a, f.call(arguments, 1)); var d = f.call(arguments, 2); return function () { return a.apply(b, d.concat(f.call(arguments))) } }; b.H = function (a) { var c = f.call(arguments, 1); c.length == 0 && (c = b.i(a)); i(c, function (c) { a[c] = b.bind(a[c], a) }); return a }; b.$ = function (a, c) {
    var d = {}; c || (c = b.d); return function () {
        var b = c.apply(this, arguments); return m.call(d, b) ? d[b] : d[b] = a.apply(this,
arguments)
    } 
}; b.o = function (a, b) { var d = f.call(arguments, 2); return setTimeout(function () { return a.apply(a, d) }, b) }; b.defer = function (a) { return b.o.apply(b, [a, 1].concat(f.call(arguments, 1))) }; b.ka = function (a, b) { return u(a, b, !1) }; b.K = function (a, b) { return u(a, b, !0) }; b.ca = function (a) { var b = !1, d; return function () { if (b) return d; b = !0; return d = a.apply(this, arguments) } }; b.oa = function (a, b) { return function () { var d = [a].concat(f.call(arguments)); return b.apply(this, d) } }; b.J = function () {
    var a = f.call(arguments); return function () {
        for (var b =
f.call(arguments), d = a.length - 1; d >= 0; d--) b = [a[d].apply(this, b)]; return b[0]
    } 
}; b.F = function (a, b) { return function () { if (--a < 1) return b.apply(this, arguments) } }; b.keys = I || function (a) { if (a !== Object(a)) throw new TypeError("Invalid object"); var b = [], d; for (d in a) m.call(a, d) && (b[b.length] = d); return b }; b.D = function (a) { return b.map(a, b.d) }; b.i = b.aa = function (a) { return b.filter(b.keys(a), function (c) { return b.t(a[c]) }).sort() }; b.extend = function (a) {
    i(f.call(arguments, 1), function (b) {
        for (var d in b) b[d] !== void 0 &&
(a[d] = b[d])
    }); return a
}; b.L = function (a) { i(f.call(arguments, 1), function (b) { for (var d in b) a[d] == h && (a[d] = b[d]) }); return a }; b.I = function (a) { return b.isArray(a) ? a.slice() : b.extend({}, a) }; b.ia = function (a, b) { b(a); return a }; b.isEqual = function (a, c) {
    if (a === c) return !0; var d = typeof a; if (d != typeof c) return !1; if (a == c) return !0; if (!a && c || a && !c) return !1; if (a.b) a = a.a; if (c.b) c = c.a; if (a.isEqual) return a.isEqual(c); if (b.k(a) && b.k(c)) return a.getTime() === c.getTime(); if (b.l(a) && b.l(c)) return !1; if (b.m(a) && b.m(c)) return a.source ===
c.source && a.global === c.global && a.ignoreCase === c.ignoreCase && a.multiline === c.multiline; if (d !== "object") return !1; if (a.length && a.length !== c.length) return !1; var d = b.keys(a), e = b.keys(c); if (d.length != e.length) return !1; for (var f in a) if (!(f in c) || !b.isEqual(a[f], c[f])) return !1; return !0
}; b.X = function (a) { if (b.isArray(a) || b.v(a)) return a.length === 0; for (var c in a) if (m.call(a, c)) return !1; return !0 }; b.W = function (a) { return !!(a && a.nodeType == 1) }; b.isArray = o || function (a) { return H.call(a) === "[object Array]" }; b.s =
function (a) { return !(!a || !m.call(a, "callee")) }; b.t = function (a) { return !(!a || !a.constructor || !a.call || !a.apply) }; b.v = function (a) { return !!(a === "" || a && a.charCodeAt && a.substr) }; b.u = function (a) { return !!(a === 0 || a && a.toExponential && a.toFixed) }; b.l = function (a) { return a !== a }; b.V = function (a) { return a === !0 || a === !1 }; b.k = function (a) { return !(!a || !a.getTimezoneOffset || !a.setUTCFullYear) }; b.m = function (a) { return !(!a || !a.test || !a.exec || !(a.ignoreCase || a.ignoreCase === !1)) }; b.Y = function (a) { return a === h }; b.Z = function (a) {
    return a ===
void 0
}; b.ba = function () { s.f = G; return this }; b.d = function (a) { return a }; b.la = function (a, b, d) { for (var e = 0; e < a; e++) b.call(d, e) }; b.z = function (a) { i(b.i(a), function (c) { E(c, b[c] = a[c]) }) }; var J = 0; b.ma = function (a) { var b = J++; return a ? a + b : b }; b.B = { evaluate: /<%([\s\S]+?)%>/g, r: /<%=([\s\S]+?)%>/g }; b.ja = function (a, c) {
    var d = b.B, d = "var __p=[],print=function(){__p.push.apply(__p,arguments);};with(obj||{}){__p.push('" + a.replace(/\\/g, "\\\\").replace(/'/g, "\\'").replace(d.r, function (a, b) {
        return "'," + b.replace(/\\'/g, "'") +
",'"
    }).replace(d.evaluate || h, function (a, b) { return "');" + b.replace(/\\'/g, "'").replace(/[\r\n\t]/g, " ") + "__p.push('" }).replace(/\r/g, "\\r").replace(/\n/g, "\\n").replace(/\t/g, "\\t") + "');}return __p.join('');", d = new Function("obj", d); return c ? d(c) : d
}; b.prototype = l.prototype; b.z(b); i("pop,push,reverse,shift,sort,splice,unshift".split(","), function (a) { var b = j[a]; l.prototype[a] = function () { b.apply(this.a, arguments); return q(this.a, this.b) } }); i(["concat", "join", "slice"], function (a) {
    var b = j[a]; l.prototype[a] =
function () { return q(b.apply(this.a, arguments), this.b) } 
}); l.prototype.n = function () { this.b = !0; return this }; l.prototype.value = function () { return this.a } 
})();