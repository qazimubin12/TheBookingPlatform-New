/*
+--------------------------------------------------------------------+
                                                  
                                                  
/-`                                            `-/
dddhso/-`                                `-/oshddd
dddddddddhso/-`                    `-/oshddddddddd
dddddddddddddddhso++++++++++++++oshddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
ddddddddddddds++sdddddddddddddddds++sddddddddddddd
dddddddddddd-    -dddddddddddddd-    -dddddddddddd
dddddddddddd-    -dddddddddddddd-    -dddddddddddd
ddddddddddddds++sdddddddddddddddds++sddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd
dddddddddddddddddddddddddddddddddddddddddddddddddd


+--------------------------------------------------------------------+

    felixg.io - dateDropper Javascript

    Version: 1.1.0
    Realase: 2023-10-01
    Created by: Felice Gattuso
    Twitter: @felixggi 
    Instagram: @felixggi
    Docs: https://felixg.io/docs/guides/datedropper-javascript
    
+--------------------------------------------------------------------+

*/
var dateDropperSetup = {
    languages: {
        en: {
            m: {
                s: ["Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sept", "Oct", "Nov", "Dec"],
                f: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]
            },
            w: {
                s: ["S", "M", "T", "W", "T", "F", "S"],
                f: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]
            }
        }
    }
};
! function () {
    this.dateDropper = function () {
        var e = dateDropper.prototype;
        e.fetch = function () {
            d.els = document.querySelectorAll(d.options.selector), d.els.length && d.els.forEach((function (e, t) {
                d.prepare(e)
            }))
        };
        var d = {};

        function t(e, d) {
            return [31, e % 4 == 0 && (e % 100 != 0 || e % 400 == 0) ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][d - 1]
        }

        function o(e) {
            return e < 10 ? "0" + e : e
        }

        function n(e) {
            return Date.parse(e) / 1e3
        }

        function a(e) {
            if (!e) return !1;
            var d = [];
            return ("string" == typeof e || e instanceof String) && (e = e.split(",")), !!Array.isArray(e) && (e.forEach((function (e) {
                d.push(n(new Date(e)))
            })), d)
        }

        function r(e, d) {
            return !(!e || !d) && (e.y == d.y && e.m == d.m && e.d == d.d)
        }

        function i(e) {
            return !!e && e.y + "/" + o(e.m) + "/" + o(e.d)
        }

        function l(e) {
            return !!e && new Date(i(e))
        }

        function s(e) {
            return !!e && {
                d: e.getDate(),
                m: e.getMonth() + 1,
                y: e.getFullYear()
            }
        }

        function c(e) {
            return i(s(e))
        }

        function p(e) {
            return !!e && new Date(e)
        }

        function u(e) {
            return s(p(e))
        }

        function g(e) {
            return !!e.arr && e.arr.includes(n(e.date))
        }

        function m(e) {
            if (e) {
                // Get the child nodes of the element
                const childNodesCount = e.childNodes.length;
                e.classList.remove("dd-shown");
                setTimeout(() => {
                    e.remove();
                    console.log(`Number of child nodes: ${childNodesCount}`);
                }, 300);

            }
        }

        function f(e, d) {
            var t = e.querySelectorAll("." + d);
            t && t.forEach((function (e) {
                e.classList.remove(d)
            }))
        }

        function b(e, d) {
            e.setAttribute("data-dd-opt-default-date", i(d))
        }

        function v(e, d) {
            e && e instanceof HTMLInputElement && (e.value = d)
        }

        function h(e, d) {
            var t = document.createElement(e);
            return d && (d.class && (t.className = d.class), d.html && (t.innerHTML = d.html), d.id && (t.id = d.id)), t
        }

        function y(e) {
            e.classList.add("dd-rumble"), setTimeout((function () {
                e.classList.remove("dd-rumble")
            }), 400)
        }

        function w(e, d) {
            for (var t in d) try {
                d[t].constructor == Object ? e[t] = _.extend(e[t], d[t]) : e[t] = d[t]
            } catch (o) {
                e[t] = d[t]
            }
            return e
        }
        d.options = {
            format: "y-mm-dd",
            lang: "en",
            blocks: ["m", "d", "y"],
            autoFill: !0,
            jump: 10,
            loopAll: !0,
            loopDays: !0,
            loopMonths: !0,
            loopYears: !0,
            highlightWeekend: !0,
            defaultBehavior: !0,
            showArrowsOnHover: !0
        }, w(d.options, arguments[0]), d.inline = d.options.inline, d.prepare = function (e) {
            if (e.classList.contains("dd__trigger")) return !1;
            e.classList.add("dd__trigger"), e instanceof HTMLInputElement && (e.setAttribute("readonly", !0), e.setAttribute("type", "text"));
            var t = d.methods(e);
            d.inline ? e.datedropper("show") : (t.triggerFunc = function () {
                this.classList.contains("dd-focused") || this.datedropper("show")
            }, e.addEventListener("click", t.triggerFunc))
            },
            d.destroy = function (e) {
            d.exit(e), e.trigger.removeEventListener("click", e.triggerFunc), delete e.trigger.datedropper, e.options.onDestroy && e.options.onDestroy(e.trigger)
            },
            d.methods = function (e) {
            var t = {};
            return t.trigger = e, Object.defineProperty(t.trigger, "datedropper", {
                value: function (e, o) {
                    switch (e) {
                        case "show":
                            d.open(t);
                            break;
                        case "hide":
                            d.exit(t);
                            break;
                        case "destroy":
                            d.destroy(t);
                            break;
                        case "set":
                            d.set(t, o)
                    }
                },
                configurable: !0
            }), t
        }, d.set = function (e, t) {
            Object.keys(t).forEach((function (d) {
                var o = d.replace(/[A-Z]/g, (e => "-" + e.toLowerCase()));
                e.trigger.setAttribute("data-dd-opt-" + o, t[d])
            })), e.dropdown && d.open(e)
        }, d.open = function (e) {
            e.dropdown && d.exit(e), e.options = d.getOptions(e), d.cleanOptions(e), d.dropdown(e), d.change(e, !0)
        }, d.locked = function (e, d) {
            e.primaryButton && (d ? (y(e.primaryButton), e.primaryButton.setAttribute("disabled", "")) : e.primaryButton.removeAttribute("disabled"))
        }, d.dataOptions = function (e) {
            for (var d = e.trigger.attributes, t = d.length, o = {}; t--;) {
                var n = d[t];
                if (n.value && n.name.includes("data-dd-opt-")) {
                    var a = n.name.replace("data-dd-opt-", "").replace(/(?:^\w|[A-Z]|\b\w)/g, (function (e, d) {
                        return 0 === d ? e.toLowerCase() : e.toUpperCase()
                    })).replace(/\s+/g, "").replace("-", ""),
                        r = n.value;
                    switch (r) {
                        case "true":
                            r = !0;
                            break;
                        case "false":
                            r = !1
                    }
                    o[a] = r
                }
            }
            return o
        }, d.getOptions = function (e) {
            var t = w({}, d.options);
            switch (t = w(t, d.dataOptions(e)), ["maxYear", "minYear"].forEach((function (e) {
                t[e] = t[e] || (new Date).getFullYear() + ("maxYear" == e ? 50 : -50)
            })), ["maxDate", "minDate"].forEach((function (e) {
                t[e] = "today" == t[e] ? c(new Date) : t[e]
            })), t.defaultDate = p(t.defaultDate) || new Date, t.range = t.range || Boolean(t.rangeStart || t.rangeEnd), t.expandedOnly = t.expandedOnly || t.range, t.expandedDefault = t.expandedDefault || t.expandedOnly, t.expandable = t.expandable || t.expandedDefault || t.doubleView, ["onlyMonth", "onlyYear"].includes(t.preset) && (t.range = !1, t.defaultBehavior = !1, t.expandable = !1, t.customClass = (t.customClass || "") + " dd-preset-" + t.preset), t.preset) {
                case "onlyMonth":
                    t.blocks = ["m"], t.format = ["m", "mm", "M", "MM"].includes(t.format) ? t.format : "m";
                    break;
                case "onlyYear":
                    t.blocks = ["y"], t.format = "y"
            }
            return t
        }, d.fixDate = function (e, d) {
            var t = e.options.maxYear,
                o = e.options.minYear,
                n = d.input.getFullYear();
            return t && n > t && (n = d.fill ? t : o), o && n < o && (n = d.fill ? o : t), d.input.setFullYear(n), d.input
        }, d.cleanOptions = function (e) {
            e.date = d.fixDate(e, {
                input: e.options.defaultDate,
                fill: !0
            }), e.options.rangeStart && e.options.rangeEnd && d.valid(e, p(e.options.rangeStart)) && d.valid(e, p(e.options.rangeEnd)) && p(e.options.rangeEnd) >= p(e.options.rangeStart) && (e.options.range = !0, e.range = {
                a: u(e.options.rangeStart),
                b: u(e.options.rangeEnd)
            }, e.date = l(e.range.a), e.selected = new Date(c(e.date))), e.options.range || (e.selected = new Date(c(e.date)))
        }, d.lang = function (e) {
            var d = dateDropperSetup.languages;
            return d[e.options.lang] || d.en
        }, d.output = function (e, t) {
            var a, r, i, l = (t = t || {}).input || e.selected || e.date,
                s = l.getFullYear(),
                c = l.getMonth() + 1,
                p = l.getDate(),
                u = l.getDay(),
                g = d.lang(e),
                m = {
                    d: p,
                    dd: o(p),
                    m: c,
                    mm: o(c),
                    M: g.m.s[c - 1],
                    MM: g.m.f[c - 1],
                    y: s,
                    w: u,
                    W: g.w.f[u].substr(0, 3),
                    WW: g.w.f[u],
                    S: (a = p, r = ["th", "st", "nd", "rd"], i = a % 100, a + (r[(i - 20) % 10] || r[i] || r[0])),
                    U: n(l)
                };
            return m.string = (t.format || e.options.format).replace(/\b(d)\b/g, m.d).replace(/\b(dd)\b/g, m.dd).replace(/\b(m)\b/g, m.m).replace(/\b(mm)\b/g, m.mm).replace(/\b(M)\b/g, m.M).replace(/\b(MM)\b/g, m.MM).replace(/\b(y)\b/g, m.y).replace(/\b(w)\b/g, m.w).replace(/\b(W)\b/g, m.W).replace(/\b(WW)\b/g, m.WW).replace(/\b(S)\b/g, m.S).replace(/\b(U)\b/g, m.U), m
        }, d.offset = function (e) {
            var t, o, n = document.querySelectorAll(d.options.selector),
                a = e.trigger.getBoundingClientRect(),
                r = e.dropdown.getBoundingClientRect().width / 2,
                i = document.documentElement.scrollTop;
            t = a.top + i + a.height + 16, e.dropdown.style.top = t + "px";
            var l = document.documentElement.scrollLeft;
            if (e.options.range && 2 == n.length) {
                var s = n[0].getBoundingClientRect(),
                    c = n[1].getBoundingClientRect(),
                    p = l + s.left;
                o = p + (l + c.left + c.width - p) / 2
            } else {
                o = a.left + l + a.width / 2
            }
            0 > o - r && (o = 16 + r), o + r + 16 > window.innerWidth && (o = window.innerWidth - 16 - r), e.dropdown.style.left = o + "px"
        }, d.block = function (e, t) {
            var o = h("div", {
                class: "dd__block",
                html: '<div class="dd__nav dd-left dd-hidden">' + d.svg("arrowLeft") + '</div><div class="dd__nav dd-right dd-hidden">' + d.svg("arrowRight") + '</div><div class="dd__view"></div>'
            });
            o.setAttribute("data-key", t), o.querySelector(".dd-left").addEventListener("click", (function () {
                d.move(e, t, !1)
            })), o.querySelector(".dd-right").addEventListener("click", (function () {
                d.move(e, t, !0)
            })), e.options.defaultBehavior && o.querySelector(".dd__view").addEventListener("click", (function () {
                switch (t) {
                    case "y":
                        d.yearDialog(e);
                        break;
                    case "m":
                        d.monthDialog(e);
                        break;
                    case "d":
                        d.toggleView(e)
                }
            })), e.dropdown.appendChild(o)
        }, d.toggleView = function (e) {
            e.options.expandable && (e.expanded = !e.expanded, d.open(e))
        }, d.calendarHeader = function (e) {
            for (var t = d.lang(e), o = h("div", {
                class: "dd__header"
            }), n = 0; n <= 6; n++) {
                var a = n,
                    r = h("div", {
                        class: "dd__item"
                    });
                e.options.startFromMonday && (a = a + 1 == 7 ? 0 : a + 1), r.innerHTML = t.w.s[a], o.appendChild(r)
            }
            return o
        }, d.validRange = function (e, d, t) {
            var o, n, a = !0,
                r = (o = d, n = t, Math.round((n - o) / 864e5));
            return a && e.options.minRange && (a = r >= e.options.minRange), a && e.options.maxRange && (a = r <= e.options.maxRange), a
        }, d.setRange = function (e, t) {
            var o = t.date,
                n = t.item;
            if (e.selecting) {
                var a = l(e.range.a),
                    r = l(o);
                if (r >= a) {
                    if (!d.validRange(e, a, r)) return void y(n);
                    e.range.b = o, e.selecting = !1
                } else e.range.a = o
            } else e.range = {}, e.range.a = o, e.selecting = !0, d.setDate(e, {
                d: o.d,
                m: o.m,
                y: o.y
            });
            e.options.onRangeSet && e.options.onRangeSet(d.prepareOutput(e))
        }, d.calendarDay = function (e, t) {
            var o = l(t.date),
                a = h("div", {
                    html: '<div class="dd-value">' + t.date.d + "</div>",
                    class: "dd__item " + (t.class || "")
                });
            d.weekend(e, o.getDay()) && a.classList.add("dd-weekend");
            var i = r(s(new Date), t.date),
                c = r(s(e.selected), t.date);
            if (!c && i && a.classList.add("dd-today"), e.options.range && e.range) {
                if (["a", "b"].forEach((function (d) {
                    e.range[d] && r(t.date, e.range[d]) && (e.range.a && e.range.b && l(e.range.a) != l(e.range.b) && a.classList.add("dd-" + d), a.classList.add("dd-selected"))
                })), e.range.a && e.range.b) {
                    var p = n(l(e.range.a)),
                        u = n(l(e.range.b)),
                        g = n(o);
                    g > p && g < u && a.classList.add("dd-point")
                }
            } else c && a.classList.add("dd-selected");
            if ((!(e.range && !e.range.b) || d.validRange(e, l(e.range.a), l(t.date))) && d.valid(e, o)) {
                var m = {
                    item: a,
                    date: t.date
                };
                e.options.onRender && d.putOnSchema(e, m), a.addEventListener("click", (function () {
                    e.options.range ? d.setRange(e, m) : d.setDate(e, {
                        d: t.date.d,
                        m: t.date.m,
                        y: t.date.y
                    }), d.change(e, !0)
                })), e.options.range && (a.addEventListener("mouseenter", (function () {
                    d.settingRange(e, m), e.dropdown.classList.add("dd-selecting"), a.classList.add("dd-selecting")
                })), a.addEventListener("mouseleave", (function () {
                    f(e.dropdown, "dd-selecting"), f(e.dropdown, "dd-starting"), f(e.dropdown, "dd-perEnd"), f(e.dropdown, "dd-perStart")
                })))
            } else a.classList.add("dd-disabled");
            return a
        }, d.putOnSchema = function (e, d) {
            var t = {
                node: d.item,
                customLabel: function (e) {
                    if (d.item.setAttribute("data-dd-tooltip", e.label), e.color) {
                        var t = h("div", {
                            class: "dd-color"
                        });
                        t.setAttribute("style", "background-color:" + e.color)
                    }
                    d.item.appendChild(t)
                }
            };
            e.CSCHEMA[i(d.date)] = t
        }, d.settingRange = function (e, t) {
            if (e.range && e.range.a && !e.range.b && l(e.range.a) < l(t.date)) {
                var o = t.item.parentNode.querySelector(".dd__item.dd-selected") || t.item.parentNode.querySelector(".dd__body .dd__item:first-of-type");
                if (d.isDoubleView(e)) {
                    var n = t.item.parentNode.parentNode.previousSibling;
                    if (n)
                        if (n.querySelector(".dd__item.dd-selected")) n.classList.add("dd-perEnd");
                        else l(t.date).getMonth() != l(e.range.a).getMonth() && n.classList.add("dd-perStart")
                }
                o.classList.add("dd-starting")
            }
        }, d.calendarBody = function (e, o) {
            var n = h("div", {
                class: "dd__body"
            }),
                a = o.date || d.output(e),
                r = t(a.y, a.m),
                i = a.y + "/" + a.m + "/01",
                l = new Date(i);
            l.setDate(l.getDate() - 1);
            var s = new Date(i);
            s.setMonth(s.getMonth() + 1);
            var c = 42,
                p = l.getDay() + (e.options.startFromMonday ? -1 : 0);
            if (6 != p)
                for (var u = p; u >= 0; u--) n.appendChild(d.calendarDay(e, {
                    class: "dd-placeholder dd-before",
                    date: {
                        d: l.getDate() - u,
                        m: l.getMonth() + 1,
                        y: l.getFullYear()
                    }
                })), c--;
            for (u = 1; u <= r; u++) n.appendChild(d.calendarDay(e, {
                class: "dd-i",
                date: {
                    d: u,
                    m: a.m,
                    y: a.y
                }
            })), c--;
            for (u = 1; u <= c; u++) n.appendChild(d.calendarDay(e, {
                class: "dd-placeholder",
                date: {
                    d: u,
                    m: s.getMonth() + 1,
                    y: s.getFullYear()
                }
            }));
            return n
        }, d.createCalendar = function (e, t) {
            var o = h("div");
            return o.appendChild(d.calendarHeader(e)), o.appendChild(d.calendarBody(e, t)), o
        }, d.calendar = function (e) {
            var t = e.dropdown.querySelector(".dd__calendar");
            if (!t) return !1;
            t.innerHTML = "", e.CSCHEMA = {};
            var o = s(e.date);
            if (t.appendChild(d.createCalendar(e, {
                date: o
            })), d.isDoubleView(e)) {
                var n = l(o);
                n.setMonth(n.getMonth() + 1), n = s(n), t.appendChild(d.createCalendar(e, {
                    date: n
                }))
            }
        }, d.prepareOutput = function (e) {
            return e.options.range ? {
                a: d.output(e, {
                    input: e.range ? l(e.range.a) : e.selected
                }),
                b: d.output(e, {
                    input: e.range ? l(e.range.b || e.range.a) : e.selected
                })
            } : d.output(e)
        }, d.save = function (e, t) {
            e.options.range ? d.saveValues(e) : d.saveValue(e), e.options.onChange && e.options.onChange({
                trigger: e.trigger,
                dropdown: e.dropdown,
                output: d.prepareOutput(e)
            }), t && e.options.onSubmit && e.options.onSubmit({
                trigger: e.trigger,
                dropdown: e.dropdown,
                output: d.prepareOutput(e)
            })
        }, d.checkoutDay = function (e, d) {
            return e.options.checkout && ((d = l(d)).setDate(d.getDate() + 1), d = s(d)), d
        }, d.saveValues = function (e) {
            e.range && ["a", "b"].forEach((function (t, o) {
                var n = e.range[t] || d.checkoutDay(e, e.range.a);
                if (n && (d.els.forEach((function (e) {
                    var d = "data-dd-opt-range-" + ("a" == t ? "start" : "end");
                    e.setAttribute(d, i(n))
                })), 2 == d.els.length && d.els[o])) {
                    var a = d.output(e, {
                        input: l(n)
                    });
                    b(d.els[o], a), v(d.els[o], a.string)
                }
            }))
        }, d.saveValue = function (e) {
            var t = d.output(e);
            b(e.trigger, t), v(e.trigger, t.string), e.options.changeValueTo && v(document.querySelector(e.options.changeValueTo), t.string)
        }, d.change = function (e, t) {
            e.dropdown && (d.fillBlocks(e, {
                input: e.date
            }), e.options.expandable && e.expanded && d.calendar(e)), d.valid(e) ? (d.locked(e, !1), e.options.autoFill && d.save(e)) : d.locked(e, !0), t && e.expanded && e.options.onRender && e.options.onRender(e.CSCHEMA)
        }, d.valid = function (e, d) {
            if (!(d = d || e.selected)) return d;
            var t = !0,
                o = e.options.enabledDays,
                r = e.options.disabledDays;
            t && !o && r && (t = !g({
                date: d,
                arr: a(r)
            })), t && !r && o && (t = g({
                date: d,
                arr: a(o)
            }));
            var i = e.options.maxDate,
                l = e.options.minDate;
            t && l && (t = n(d) >= n(l)), t && i && (t = n(d) <= n(i));
            var s = e.options.disabledWeekDays;
            return t && s && (t = !s.includes(d.getDay())), t
        }, d.loopSet = function (e, d) {
            var t = !e.options.loopAll;
            switch (d) {
                case "y":
                    t = t || !e.options.loopYears;
                    break;
                case "m":
                    t = t || !e.options.loopMonths;
                    break;
                case "d":
                    t = t || !e.options.loopDays
            }
            return t
        }, d.lockedLoop = function (e, o, n) {
            var a = !0,
                r = s(e.date);
            switch (o) {
                case "y":
                    a = d.loopSet(e, "y") && r.y == (n ? e.options.maxYear : e.options.minYear);
                    break;
                case "m":
                    a = d.loopSet(e, "m") && r.m == (n ? 12 : 1);
                    break;
                case "d":
                    a = d.loopSet(e, "d") && r.d == (n ? t(r.y, r.m) : 1)
            }
            return a
        }, d.move = function (e, t, o) {
            var n = e.date,
                a = o ? 1 : -1;
            if (d.lockedLoop(e, t, o)) return !1;
            switch (t) {
                case "y":
                    n.setFullYear(n.getFullYear() + a);
                    break;
                case "m":
                    n.setMonth(n.getMonth() + a);
                    break;
                case "d":
                    n.setDate(n.getDate() + a)
            }
            n = d.fixDate(e, {
                input: n
            }), e.expanded || (e.selected = new Date(c(e.date))), d.change(e, !0)
        }, d.isDoubleView = function (e) {
            return e.options.doubleView && window.innerWidth > 768 ? (e.dropdown.classList.add("dd-doubleView"), !0) : (e.dropdown.classList.remove("dd-doubleView"), !1)
        }, d.weekend = function (e, d) {
            return e.options.highlightWeekend && [0, 6].includes(d)
        }, d.fillBlocks = function (e) {
            var t = d.output(e, {
                input: e.date
            }),
                o = t.M;
            if (e.expanded && d.isDoubleView(e)) {
                var n = new Date(c(e.date));
                n.setMonth(n.getMonth() + 1), o = o + " - " + (n = d.output(e, {
                    input: n
                })).M
            }
            var a = {
                y: "<div>" + t.y + "</div>",
                m: "<div>" + o + "</div>",
                d: "<div>" + t.dd + "</div><div " + (d.weekend(e, t.w) ? 'class="dd-w"' : "") + '">' + t.WW + "</div>"
            };
            e.options.blocks.forEach((function (d) {
                e.dropdown.querySelector('.dd__block[data-key="' + d + '"] .dd__view').innerHTML = a[d]
            }))
        }, d.monthDialog = function (e) {
            d.dialog(e, {
                onDom: function (t) {
                    for (var o = 0; o <= 11; o++) t.appendChild(d.monthDialogItem(e, {
                        dialog: t,
                        value: o,
                        timeout: 50 * o
                    }))
                }
            })
        }, d.monthDialogItem = function (e, t) {
            var o = d.lang(e);
            return d.dialogItem(e, {
                class: "dd__item dd-hidden",
                html: o.m.s[t.value],
                timeout: t.timeout,
                onClick: function () {
                    d.setDate(e, {
                        m: t.value + 1
                    }, !0), d.change(e, !0), d.exitDialog(e, t.dialog)
                }
            })
        }, d.yearDialogLoop = function (e, t) {
            var o = 0;
            if (!t.init) {
                var n = d.dialogItem(e, {
                    html: "...",
                    class: "dd__item dd-hidden",
                    timeout: 0,
                    onClick: function () {
                        d.exitDialog(e, t.dialog), d.yearDialog(e)
                    }
                });
                t.dialog.appendChild(n)
            }
            for (var a = t.min; a <= t.max; a++)
                if (!t.multiple || t.multiple && a % t.multiple == 0) {
                    var r = d.yearDialogItem(e, {
                        dialog: t.dialog,
                        value: a,
                        init: t.init,
                        timeout: 50 * o
                    });
                    t.dialog.appendChild(r), o++
                }
        }, d.yearDialog = function (e) {
            e.options.maxYear - e.options.minYear < 20 || d.dialog(e, {
                onDom: function (t) {
                    d.yearDialogLoop(e, {
                        min: e.options.minYear,
                        max: e.options.maxYear,
                        multiple: e.options.jump,
                        dialog: t,
                        init: !0
                    })
                }
            })
        }, d.yearDialogItem = function (e, t) {
            return d.dialogItem(e, {
                html: t.value,
                class: "dd__item dd-hidden",
                timeout: t.timeout,
                onClick: function () {
                    t.init ? (t.dialog.innerHTML = "", t.dialog.scrollTop = 0, d.yearDialogLoop(e, {
                        min: t.value,
                        max: t.value + 10,
                        dialog: t.dialog
                    })) : (d.setDate(e, {
                        y: t.value
                    }, !0), d.change(e, !0), d.exitDialog(e, t.dialog))
                }
            })
        }, d.setDate = function (e, d, t) {
            var o = s(e.selected || e.date);
            e.selected = new Date((d.y || o.y) + "/" + (d.m || o.m) + "/" + (d.d || o.d)), !t && e.expanded || (e.date = new Date(c(e.selected)))
        }, d.dialogItem = function (e, d) {
            var t = h("div", {
                class: d.class,
                html: d.html
            });
            return d.onClick && t.addEventListener("click", d.onClick), setTimeout((function () {
                t.classList.add("dd-shown")
            }), d.timeout || 0), t
        }, d.dialog = function (e, t) {
            var o = h("div", {
                class: "dd__dialog dd-hidden"
            });
            t.onDom && t.onDom(o);
            var n = h("div", {
                class: "dd__footer"
            });
            n.addEventListener("click", (function () {
                d.exitDialog(e, o)
            })), o.appendChild(n), /*e.dropdown.appendChild(o),*/
                e.dropdown.classList.add("dd--dialog-shown"),
                setTimeout((function () {
                o.classList.add("dd-shown")
            }), 50)
        }, d.exitDialog = function (e, d) {
            e.dropdown.classList.remove("dd--dialog-shown"), m(d)
        }, d.dropdown = function (e) {
            if (e.dropdown = h("div", {
                class: "dd__dropdown dd-hidden " + (e.options.customClass || "") + (e.options.showArrowsOnHover ? " dd-arw-hover" : "") + (d.inline ? " dd-inline" : ""),
                id: "datedropper"
            }), e.options.overlay && (e.overlay = h("div", {
                class: "dd__overlay dd-hidden"
            }), document.body.appendChild(e.overlay)), e.options.blocks.forEach((function (t) {
                d.block(e, t)
            })), !d.inline) {
                var t = h("div", {
                    class: "dd__footer"
                });
                e.dropdown.appendChild(t), e.primaryButton = h("div", {
                    class: "dd__primaryButton",
                    html: d.svg("checkmark")
                }), e.primaryButton.addEventListener("click", (function () {
                    d.save(e, !0), d.exit(e)
                })), t.appendChild(e.primaryButton)
            }
            if (e.options.expandable) {
                if (!e.options.expandedOnly && !d.inline) {
                    var o = h("div", {
                        class: "dd__expandButton",
                        html: e.expanded ? d.svg("reduce") : d.svg("expand")
                    });
                    o.addEventListener("click", (function () {
                        d.toggleView(e)
                    })), e.dropdown.appendChild(o)
                }
                if (e.expanded = !(void 0 !== e.expanded || !e.options.expandedDefault) || e.expanded, e.expanded) {
                    e.dropdown.classList.add("dd-expanded"), d.isDoubleView(e);
                    var n = e.dropdown.querySelector("[data-key=d]");
                    if (n) {
                        var a = h("div", {
                            class: "dd__calendar"
                        });
                        n.parentNode.insertBefore(a, n)
                    }
                }
            }
            d.inline ? (e.trigger.innerHTML = "", e.trigger.appendChild(e.dropdown)) : (e.trigger.classList.add("dd-focused"), document.body.appendChild(e.dropdown)), e.onResize = function (t) {
                if (d.inline || d.offset(e), e.expanded && e.options.doubleView) {
                    var o = e.dropdown.classList.contains("dd-doubleView");
                    (window.innerWidth < 768 && o || window.innerWidth > 768 && !o) && e.trigger.datedropper("show")
                }
            }, window.addEventListener("resize", e.onResize), d.inline ? (e.onResize(), e.dropdown.classList.add("dd-shown")) : (e.onBlur = function (t) {
                (t = t || window.event).target == e.trigger || t.target == e.dropdown || e.dropdown.contains(t.target) || d.exit(e)
            }, document.addEventListener("mousedown", e.onBlur), setTimeout((function () {
                e.onResize(), e.dropdown.classList.add("dd-shown"), e.overlay && e.overlay.classList.add("dd-shown"), e.options.onDropdownOpen && e.options.onDropdownOpen({
                    trigger: e.trigger,
                    dropdown: e.dropdown,
                    output: d.prepareOutput(e)
                })
            }), 50))
        }, d.svg = function (e) {
            return '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24">' + {
                arrowLeft: '<path d="M16 21l-9-9 9-9"/>',
                arrowRight: '<path d="M7 21l9-9-9-9"/>',
                checkmark: '<path d="M2.998 11.049l6.965 6.942 11.035-11"/>',
                close: '<path d="M5.5 5.5l13 13M18.5 5.5l-13 13"/>',
                expand: '<g fill="none" fill-rule="evenodd" stroke="currentColor" stroke-width="2"><path stroke-linecap="square" d="M19 5l-4.643 4.643"/><path d="M12 4h8v8"/><path stroke-linecap="square" d="M5 19l4.643-4.643"/><path d="M12 20H4v-8"/></g>',
                reduce: '<g fill="none" fill-rule="evenodd" stroke="currentColor" stroke-width="2"><path stroke-linecap="square" d="M4 21l6-6"/><path d="M3 14h8v8"/><path stroke-linecap="square" d="M21 3l-6 6"/><g><path d="M22 10h-8V2"/></g></g>'
            }[e] + "</svg>"
        }, d.exit = function (e) {
            d.inline || (m(e.dropdown), m(e.overlay), e.dropdown = !1, e.options.onDropdownExit && e.options.onDropdownExit({
                trigger: e.trigger,
                dropdown: e.dropdown,
                output: d.prepareOutput(e)
            }), document.removeEventListener("mousedown", e.onBlur), window.removeEventListener("resize", e.onResize), e.trigger.classList.remove("dd-focused"))
        }, e.fetch()
    };
    var e = document.createElement("style"),
        d = ':root{--dd-overlay:rgba(0,0,0,.75);--dd-background:#FFFFFF;--dd-text1:#333333;--dd-text2:#FFFFFF;--dd-primary:#FD4741;--dd-gradient:linear-gradient(45deg,#e61e68 0%,#FD4741 100%);--dd-radius:.35em;--dd-shadow:0 0 2.5em rgba(0,0,0,0.1);--dd-range:rgba(0,0,0,0.05);--dd-monthBackground:var(--dd-gradient);--dd-monthText:var(--dd-text2);--dd-monthBorder:transparent;--dd-confirmButtonBackground:var(--dd-gradient);--dd-confirmButtonText:var(--dd-text2);--dd-selectedBackground:var(--dd-gradient);--dd-selectedText:var(--dd-text2);--dd-borderColor:rgba(0,0,0,0.1)}@keyframes dd-rumble{0%,to{transform:translate3d(0,0,0)}10%,30%,50%,70%,90%{transform:translate3d(-2px,0,0)}20%,40%,60%,80%{transform:translate3d(2px,0,0)}}.dd-theme-translucent{--dd-background:#ffffff03;--dd-text1:#ffffff;--dd-text2:var(--dd-background);--dd-primary:#7CD259;--dd-primaryBackground:var(--dd-primary);--dd-shadow:0 0 0 1px rgba(255,255,255,0.1),0 0 2.5em rgba(0,0,0,0.1);--dd-range:#7CD25920;--dd-monthBackground:var(--dd-background);--dd-monthText:var(--dd-text1 );--dd-monthBorder:rgba(0,0,0,0.1);--dd-confirmButtonBackground:var(--dd-primaryBackground);--dd-confirmButtonText:#222;--dd-selectedBackground:var(--dd-primaryBackground);--dd-selectedText:#222;-webkit-backdrop-filter:blur(0.5rem);backdrop-filter:blur(0.5rem)}.dd-theme-translucent .dd__dialog{background-color:rgba(255,255,255,0.025)}.dd-theme-translucent .dd__dialog .dd__item{background-color:rgba(0,0,0,0.1)}.dd-theme-translucent .dd__dialog .dd__item:hover{background-color:rgba(0,0,0,0.2)}.dd-theme-translucent .dd__block,.dd-theme-translucent .dd__calendar,.dd-theme-translucent .dd__footer{transition:filter 0.4s ease}.dd-theme-translucent.dd--dialog-shown .dd__block,.dd-theme-translucent.dd--dialog-shown .dd__calendar,.dd-theme-translucent.dd--dialog-shown .dd__footer{filter:blur(1rem)}.dd-theme-bootstrap{--dd-background:#f8f9fa;--dd-text1:#212529;--dd-text2:var(--dd-background);--dd-primary:#0d6efd;--dd-primaryBackground:var(--dd-primary);--dd-shadow:0 0 0 1px rgba(0,0,0,0.1),0 0 2.5em rgba(0,0,0,0.1);--dd-range:#0d6efd20;--dd-monthBackground:var(--dd-background);--dd-monthText:var(--dd-text1 );--dd-monthBorder:rgba(0,0,0,0.1);--dd-confirmButtonBackground:var(--dd-primaryBackground);--dd-confirmButtonText:var(--dd-text2);--dd-selectedBackground:var(--dd-primaryBackground);--dd-selectedText:var(--dd-text2)}.dd-theme-dark{--dd-background:#2f2f2f;--dd-text1:#ffffff;--dd-text2:#ffffff;--dd-primary:#505050;--dd-range:rgba(0,0,0,0.2)}.dd-rumble{animation:dd-rumble 0.4s ease}.dd-hidden{transition:opacity 0.3s cubic-bezier(0.165,0.84,0.44,1),pointer-events 0.3s cubic-bezier(0.165,0.84,0.44,1),visibility 0.3s cubic-bezier(0.165,0.84,0.44,1),transform 0.3s cubic-bezier(0.165,0.84,0.44,1),width 0.3s cubic-bezier(0.165,0.84,0.44,1),top 0.3s cubic-bezier(0.165,0.84,0.44,1),left 0.3s cubic-bezier(0.165,0.84,0.44,1);opacity:0;pointer-events:none;visibility:hidden;will-change:opacity,visibility,transform}.dd-shown{opacity:1;pointer-events:auto;visibility:visible}.dd-w{color:var(--dd-primary)}.dd__block{position:relative;font-weight:bold;z-index:1;display:flex;align-items:center;justify-content:center}.dd__block[data-key=m]{font-weight:normal;background:var(--dd-monthBackground);color:var(--dd-monthText);border-bottom:1px solid var(--dd-monthBorder);border-top-left-radius:var(--dd-radius);border-top-right-radius:var(--dd-radius)}.dd__block[data-key=m] .dd__view>div{font-size:2em}.dd__block[data-key=d]{border-bottom:1px solid var(--dd-borderColor)}.dd__block[data-key=d] .dd__view{padding:0.75rem 0.35em}.dd__block[data-key=d] .dd__view>div:first-of-type{font-size:5em;line-height:0.65;margin-bottom:0.125em}.dd__block[data-key=d] .dd__view>div:last-of-type{font-size:1.15em}.dd__block[data-key=y] .dd__view>div{font-size:1.5em}.dd__block:hover .dd__nav{opacity:0.5;visibility:visible;pointer-events:auto}.dd__block:hover .dd__nav:hover{opacity:1}.dd__block:hover .dd__view{background-color:rgba(0,0,0,0.05)}.dd__view{padding:0.35em;margin:0.25em;border-radius:0.35em;flex:1;cursor:pointer}.dd__nav{position:absolute;top:50%;transform:translateY(-50%);display:flex;padding:1em;cursor:pointer;width:35%;z-index:1}.dd__nav.dd-left{left:0}.dd__nav.dd-right{right:0;justify-content:flex-end}.dd__nav svg{width:1em;fill:none;fill-rule:evenodd;stroke:currentColor;stroke-width:3px;transition:stroke-width 0.6s cubic-bezier(0.165,0.84,0.44,1);will-change:stroke-width}.dd__nav:hover svg{stroke-width:5px}.dd__primaryButton{width:3.5em;height:3.5em;display:flex;align-items:center;justify-content:center;border-top-left-radius:2em;border-top-right-radius:2em;transition:transform 0.6s cubic-bezier(0.165,0.84,0.44,1);will-change:box-shadow,transform;position:relative;margin:0;outline:0;border-bottom:0;background:var(--dd-confirmButtonBackground);color:var(--dd-confirmButtonText);background-size:150% 150%;background-position:center;overflow:hidden}.dd__primaryButton svg{fill:none;fill-rule:evenodd;stroke:currentColor;stroke-width:3px;transition:stroke-width 0.6s cubic-bezier(0.165,0.84,0.44,1);will-change:stroke-width;color:currentColor;width:2em}.dd__primaryButton:before{content:"";pointer-events:none;position:absolute;top:0;left:0;background-color:black;width:100%;height:100%;transition:opacity 0.6s cubic-bezier(0.165,0.84,0.44,1);opacity:0;z-index:-1}.dd__primaryButton:not([disabled]){cursor:pointer}.dd__primaryButton[disabled]{background:rgba(0,0,0,0.075);color:var(--dd-text1);pointer-events:none}.dd__primaryButton:not([disabled]):hover{transform:translateY(0.35em)}.dd__primaryButton:not([disabled]):hover:before{opacity:0.25}.dd-selecting .dd__calendar div:not(.dd-perEnd) .dd-starting:not(.dd-selected),.dd-selecting .dd__calendar div:not(.dd-perEnd) .dd-starting~.dd__item:not(.dd-selecting~.dd__item):not(.dd-b~.dd__item),.dd-selecting .dd__calendar div:not(.dd-perStart) .dd-starting:not(.dd-selected),.dd-selecting .dd__calendar div:not(.dd-perStart) .dd-starting~.dd__item:not(.dd-selecting~.dd__item):not(.dd-b~.dd__item){background-color:var(--dd-range)}.dd-selecting .dd__calendar div.dd-perEnd .dd-selected~.dd__item,.dd-selecting .dd__calendar div.dd-perStart .dd__item:first-of-type~.dd__item{background-color:var(--dd-range)}.dd__calendar{display:flex;position:relative;border-bottom:1px solid var(--dd-borderColor)}.dd__calendar>div{display:flex;flex-direction:column;transform-origin:top center;padding:0.5rem}.dd__calendar .dd__item{flex:0 0 14.2857142857%;max-width:14.2857142857%;padding:0.5em 0.65em;position:relative}.dd__calendar .dd__item,.dd__calendar .dd__item .dd-value{display:flex;align-items:center;justify-content:center;position:relative}.dd__calendar .dd__item .dd-value{font-size:0.9em}.dd__calendar .dd__item .dd-color{position:absolute;height:0.5rem;width:2rem;opacity:0.25;border-radius:0.35rem;z-index:-1}.dd__calendar .dd__item[data-dd-tooltip]:after{content:attr(data-dd-tooltip);pointer-events:none;position:absolute;box-shadow:0 0 0.5rem rgba(0,0,0,0.1);border-radius:0.35rem;background-color:var(--dd-text1);color:var(--dd-background);padding:0.5rem 1rem;top:100%;left:50%;transform:translateX(-50%) translateY(-0.5rem);z-index:2;opacity:0;visibility:hidden;transition:opacity 0.6s cubic-bezier(0.165,0.84,0.44,1),pointer-events 0.6s cubic-bezier(0.165,0.84,0.44,1),visibility 0.6s cubic-bezier(0.165,0.84,0.44,1),transform 0.6s cubic-bezier(0.165,0.84,0.44,1)}.dd__calendar .dd__item[data-dd-tooltip]:hover:after{transform:translateX(-50%) translateY(0);opacity:1;visibility:visible}.dd__calendar .dd__body,.dd__calendar .dd__header{display:flex;align-items:center;flex-wrap:wrap}.dd__calendar .dd__header{flex-shrink:0;margin-bottom:0.5rem;border-bottom:1px solid var(--dd-borderColor)}.dd__calendar .dd__header>div{opacity:0.5}.dd__calendar .dd__body{flex:1 1 auto}.dd__calendar .dd__body .dd__item{padding:0.85em 0.5em;margin-bottom:0.25rem}.dd__calendar .dd__body .dd__item:before{width:3.25em;height:3.25em;content:"";border-radius:3.25em;position:absolute;top:50%;left:50%;transform:translate(-50%,-50%);border:1px solid transparent}.dd__calendar .dd__body .dd__item.dd-selected{color:var(--dd-selectedText);position:relative;z-index:1}.dd__calendar .dd__body .dd__item.dd-selected:before{background:var(--dd-selectedBackground);background-size:150% 150%;background-position:center}.dd__calendar .dd__body .dd__item.dd-selected .dd-value{transform:scale(1.35);opacity:1;font-weight:bold}.dd__calendar .dd__body .dd__item.dd-selected.dd-a:not(.dd-b){background-color:var(--dd-range);border-top-left-radius:3.25em;border-bottom-left-radius:3.25em}.dd__calendar .dd__body .dd__item.dd-selected.dd-b:not(.dd-a){background-color:var(--dd-range);border-top-right-radius:3.25em;border-bottom-right-radius:3.25em}.dd__calendar .dd__body .dd__item:not(.dd-selected).dd-disabled .dd-value{text-decoration:line-through;opacity:0.5}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled){cursor:pointer}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled).dd-placeholder .dd-value{opacity:0.5}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled).dd-point{background-color:var(--dd-range)}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled):not(.dd-selected).dd-today{text-decoration:underline}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled):not(.dd-selected).dd-weekend{color:var(--dd-primary)}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled):not(.dd-selected):hover .dd-value{opacity:1;transform:scale(1.35)}.dd__calendar .dd__body .dd__item:not(.dd-selected):not(.dd-disabled):not(.dd-selected):hover:before{border-color:currentColor;border-style:dashed}.dd__dialog{position:absolute;top:0;left:0;width:100%;height:100%;overflow-y:auto;display:flex;flex-wrap:wrap;align-items:inherit;padding:0.25em;border-radius:var(--dd-radius);background-color:var(--dd-background);z-index:2;transform:translateY(1em)}.dd__dialog .dd__item{flex:0 0 46%;max-width:46%;margin:2%;border-radius:0.35em;padding:1em;font-size:1.25em;font-weight:bold;background-color:rgba(0,0,0,0.05);cursor:pointer;display:flex;align-items:center;justify-content:center;border:1px solid transparent;transform:translateY(1em)}.dd__dialog .dd__item.dd-shown{transform:translateY(0)}.dd__dialog .dd__item:hover{background-color:rgba(0,0,0,0.1)}.dd__dialog.dd-shown{transform:translateY(0)}.dd__expandButton{position:absolute;bottom:0.5em;right:0.5em;width:2.5em;height:2.5em;display:flex;align-items:center;justify-content:center;border-radius:0.35em;background-color:var(--dd-background);box-shadow:0 0 1em rgba(0,0,0,0.15);transition:box-shadow 0.6s cubic-bezier(0.165,0.84,0.44,1),transform 0.6s cubic-bezier(0.165,0.84,0.44,1);cursor:pointer}.dd__expandButton:hover{box-shadow:0 0 0.5em rgba(0,0,0,0.2);transform:scale(0.95)}.dd__expandButton:active{transform:scale(0.9)}.dd__overlay{position:fixed;top:0;left:0;z-index:1;width:100%;height:100%;z-index:2147483646;background-color:var(--dd-overlay)}.dd-preset-onlyMonth .dd__block[data-key=m],.dd-preset-onlyMonth .dd__header,.dd-preset-onlyYear .dd__block[data-key=m],.dd-preset-onlyYear .dd__header{background:var(--dd-background);color:var(--dd-text1)}.dd__dropdown{background-color:var(--dd-background);color:var(--dd-text1);border-radius:var(--dd-radius);box-shadow:var(--dd-shadow);margin:0;padding:0;list-style:none;font-size:16px}.dd__dropdown,.dd__dropdown *{box-sizing:border-box;-webkit-user-select:none;user-select:none;font-family:"Helvetica Neue",sans-serif;-webkit-tap-highlight-color:transparent;text-align:center;touch-action:manipulation;line-height:1.2}.dd__dropdown>.dd__footer{display:flex;align-items:center;justify-content:center;position:relative;overflow:hidden}.dd__dropdown.dd-inline{min-width:16em}.dd__dropdown:not(.dd-inline){position:absolute;z-index:2147483647;width:11em;transform:translateY(-1em) translateX(-50%)}.dd__dropdown:not(.dd-inline).dd-shown{transform:translateY(0) translateX(-50%)}@media (max-height:480px),(max-width:480px){.dd__dropdown:not(.dd-inline){position:fixed!important;top:50%!important;left:50%!important;transform:translate(-50%,-50%)!important}}.dd__dropdown:not(.dd-inline).dd-expanded{width:20em}.dd__dropdown:not(.dd-inline).dd-expanded.dd-doubleView{width:40em}.dd__dropdown:not(.dd-inline).dd-expanded.dd-doubleView .dd__calendar>div{flex:0 0 50%}.dd__dropdown:not(.dd-arw-hover) .dd__nav{opacity:0.5;visibility:visible}.dd__dropdown.dd-expanded.dd-doubleView .dd-placeholder{opacity:0;visibility:hidden;pointer-events:none}.dd__dropdown.dd-expanded.dd-doubleView .dd__calendar>div:last-of-type{box-shadow:inset 1px 0 var(--dd-borderColor)}.dd__dropdown.dd-expanded [data-key=d]{display:none}';
    e.type = "text/css", e.styleSheet ? e.styleSheet.cssText = d : e.appendChild(document.createTextNode(d)), document.head.appendChild(e)
}();