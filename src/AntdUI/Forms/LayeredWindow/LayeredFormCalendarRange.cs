﻿// COPYRIGHT (C) Tom. ALL RIGHTS RESERVED.
// THE AntdUI PROJECT IS AN WINFORM LIBRARY LICENSED UNDER THE Apache-2.0 License.
// LICENSED UNDER THE Apache License, VERSION 2.0 (THE "License")
// YOU MAY NOT USE THIS FILE EXCEPT IN COMPLIANCE WITH THE License.
// YOU MAY OBTAIN A COPY OF THE LICENSE AT
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE
// DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED.
// SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING PERMISSIONS AND
// LIMITATIONS UNDER THE License.
// GITEE: https://gitee.com/antdui/AntdUI
// GITHUB: https://github.com/AntdUI/AntdUI
// CSDN: https://blog.csdn.net/v_132
// QQ: 17379620

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AntdUI
{
    public class LayeredFormCalendarRange : ILayeredFormOpacityDown
    {
        DateTime? minDate, maxDate;
        public LayeredFormCalendarRange(DatePickerRange _control, Rectangle rect_read, DateTime[]? date, Action<DateTime[]> _action, Action<object> _action_btns, Func<DateTime[], List<DateBadge>?>? _badge_action = null)
        {
            _control.Parent.SetTopMost(Handle);
            control = _control;
            minDate = _control.MinDate;
            maxDate = _control.MaxDate;
            badge_action = _badge_action;
            PARENT = _control;
            action = _action;
            action_btns = _action_btns;
            hover_lefts = new ITaskOpacity(this);
            hover_left = new ITaskOpacity(this);
            hover_rights = new ITaskOpacity(this);
            hover_right = new ITaskOpacity(this);
            hover_year = new ITaskOpacity(this);
            hover_month = new ITaskOpacity(this);
            hover_year_r = new ITaskOpacity(this);
            hover_month_r = new ITaskOpacity(this);
            scrollY_left = new ScrollY(this);

            float dpi = Config.Dpi;
            if (dpi != 1F)
            {
                t_one_width = (int)(t_one_width * dpi);
                t_top = (int)(t_top * dpi);
                t_time = (int)(t_time * dpi);
                t_time_height = (int)(t_time_height * dpi);
                left_button = (int)(left_button * dpi);
                year_width = (int)(year_width * dpi);
                year2_width = (int)(year2_width * dpi);
                month_width = (int)(month_width * dpi);
            }
            if (_control.Presets.Count > 0)
            {
                left_buttons = new List<CalendarButton>(_control.Presets.Count);
                int y = 0;
                foreach (object it in _control.Presets)
                {
                    left_buttons.Add(new CalendarButton(y, it));
                    y++;
                }
                t_x = left_button;
            }
            t_width = t_x + t_one_width * 2;

            rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
            rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
            rect_rights = new Rectangle(t_width + 10 - t_top, 10, t_top, t_top);
            rect_right = new Rectangle(t_width + 10 - t_top * 2, 10, t_top, t_top);

            rect_year = new Rectangle(t_x + 10 + t_one_width / 2 - year_width, 10, year_width, t_top);
            rect_year2 = new Rectangle(t_x + 10 + (t_one_width - year2_width) / 2, 10, year2_width, t_top);
            rect_month = new Rectangle(t_x + 10 + t_one_width / 2, 10, month_width, t_top);

            rect_year_r = new Rectangle(rect_year.Left + t_one_width, rect_year.Y, rect_year.Width, rect_year.Height);
            rect_month_r = new Rectangle(rect_month.Left + t_one_width, rect_month.Y, rect_month.Width, rect_month.Height);

            Font = new Font(_control.Font.FontFamily, 11.2F);
            SelDate = date;
            Date = date == null ? DateNow : date[0];

            var point = _control.PointToScreen(Point.Empty);
            int r_w = t_width + 20, r_h;
            if (calendar_day == null) r_h = 348 + 20;
            else r_h = t_top + (12 * 2) + (int)Math.Ceiling((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16) / 7F) + 20;
            SetSize(r_w, r_h);
            t_h = r_h;
            Placement = _control.Placement;
            CLocation(point, _control.Placement, _control.DropDownArrow, ArrowSize, 10, r_w, r_h, rect_read, ref Inverted, ref ArrowAlign);
        }

        #region 属性

        #region 参数

        IControl control;
        int Radius = 6;
        int t_one_width = 288, t_width = 288, t_h = 0, t_x = 0, left_button = 120, t_top = 34, t_time = 56, t_time_height = 30;
        int year_width = 60, year2_width = 88, month_width = 40;
        TAlignFrom Placement = TAlignFrom.BL;
        TAlign ArrowAlign = TAlign.None;
        int ArrowSize = 8;
        List<CalendarButton>? left_buttons = null;
        ScrollY scrollY_left;
        string YearButton = Localization.Provider?.GetLocalizedString("Year") ?? "年",
            MonthButton = Localization.Provider?.GetLocalizedString("Month") ?? "月",
            MondayButton = Localization.Provider?.GetLocalizedString("Mon") ?? "一",
            TuesdayButton = Localization.Provider?.GetLocalizedString("Tue") ?? "二",
            WednesdayButton = Localization.Provider?.GetLocalizedString("Wed") ?? "三",
            ThursdayButton = Localization.Provider?.GetLocalizedString("Thu") ?? "四",
            FridayButton = Localization.Provider?.GetLocalizedString("Fri") ?? "五",
            SaturdayButton = Localization.Provider?.GetLocalizedString("Sat") ?? "六",
            SundayButton = Localization.Provider?.GetLocalizedString("Sun") ?? "日";

        /// <summary>
        /// 回调
        /// </summary>
        Action<DateTime[]> action;
        Action<object> action_btns;
        Func<DateTime[], List<DateBadge>?>? badge_action;
        Dictionary<string, DateBadge> badge_list = new Dictionary<string, DateBadge>();

        #endregion

        #region 日期

        public DateTime[]? SelDate;
        DateTime _Date, _Date_R;
        DateTime DateNow = DateTime.Now;
        List<Calendari>? calendar_year = null;
        List<Calendari>? calendar_month = null;
        List<Calendari>? calendar_day = null;
        List<Calendari>? calendar_day2 = null;
        public DateTime Date
        {
            get => _Date;
            set
            {
                _Date = value;
                _Date_R = value.AddMonths(1);
                sizeday = size_month = size_year = true;
                calendar_day = GetCalendar(value);
                calendar_day2 = GetCalendar(_Date_R);

                #region 添加月

                var _calendar_month = new List<Calendari>(12);
                int x_m = 0, y_m = 0;
                for (int i = 0; i < 12; i++)
                {
                    var d_m = new DateTime(value.Year, i + 1, 1);
                    _calendar_month.Add(new Calendari(0, x_m, y_m, d_m.ToString("MM") + MonthButton, d_m, d_m.ToString("yyyy-MM"), minDate, maxDate));
                    x_m++;
                    if (x_m > 2)
                    {
                        y_m++;
                        x_m = 0;
                    }
                }
                calendar_month = _calendar_month;

                #endregion

                #region 添加年

                int syear = value.Year - 1;
                if (!value.Year.ToString().EndsWith("0"))
                {
                    string temp = value.Year.ToString();
                    syear = int.Parse(temp.Substring(0, temp.Length - 1) + "0") - 1;
                }
                var _calendar_year = new List<Calendari>(12);
                int x_y = 0, y_y = 0;
                if (syear < 1) syear = 1;
                for (int i = 0; i < 12; i++)
                {
                    var d_y = new DateTime(syear + i, value.Month, 1);
                    _calendar_year.Add(new Calendari(i == 0 ? 0 : 1, x_y, y_y, d_y.ToString("yyyy"), d_y, d_y.ToString("yyyy"), minDate, maxDate));
                    x_y++;
                    if (x_y > 2)
                    {
                        y_y++;
                        x_y = 0;
                    }
                }
                year_str = _calendar_year[1].date_str + "-" + _calendar_year[_calendar_year.Count - 2].date_str;
                calendar_year = _calendar_year;

                #endregion

                if (badge_action != null)
                {
                    var oldval = value;
                    ITask.Run(() =>
                    {
                        var dir = badge_action(new DateTime[] { calendar_day[0].date, calendar_day[calendar_day.Count - 1].date });
                        if (_Date == oldval)
                        {
                            badge_list.Clear();
                            if (dir == null)
                            {
                                Print();
                                return;
                            }
#if NET40 || NET46 || NET48
                            foreach (var it in dir) badge_list.Add(it.Date, it);
#else
                            foreach (var it in dir) badge_list.TryAdd(it.Date, it);
#endif
                            Print();
                        }
                    });
                }

                hover_left.Enable = Helper.DateExceed(value.AddMonths(-1), minDate, maxDate);
                hover_right.Enable = Helper.DateExceed(value.AddMonths(1), minDate, maxDate);
                hover_lefts.Enable = Helper.DateExceed(value.AddYears(-1), minDate, maxDate);
                hover_rights.Enable = Helper.DateExceed(value.AddYears(1), minDate, maxDate);
            }
        }

        string year_str = "";

        bool sizeday = true, size_month = true, size_year = true;
        List<Calendari> GetCalendar(DateTime now)
        {
            var calendaris = new List<Calendari>(28);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            var now1 = new DateTime(now.Year, now.Month, 1);
            int day_ = 0;
            switch (now1.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    day_ = 1;
                    break;
                case DayOfWeek.Wednesday:
                    day_ = 2;
                    break;
                case DayOfWeek.Thursday:
                    day_ = 3;
                    break;
                case DayOfWeek.Friday:
                    day_ = 4;
                    break;
                case DayOfWeek.Saturday:
                    day_ = 5;
                    break;
                case DayOfWeek.Sunday:
                    day_ = 6;
                    break;
            }
            if (day_ > 0)
            {
                var date1 = now.AddMonths(-1);
                int days2 = DateTime.DaysInMonth(date1.Year, date1.Month);
                for (int i = 0; i < day_; i++)
                {
                    int day3 = days2 - i;
                    calendaris.Insert(0, new Calendari(0, (day_ - 1) - i, 0, day3.ToString(), new DateTime(date1.Year, date1.Month, day3), minDate, maxDate));
                }
            }
            int x = day_, y = 0;
            for (int i = 0; i < days; i++)
            {
                int day = i + 1;
                calendaris.Add(new Calendari(1, x, y, day.ToString(), new DateTime(now.Year, now.Month, day), minDate, maxDate));
                x++;
                if (x > 6)
                {
                    y++;
                    x = 0;
                }
            }
            if (x < 7)
            {
                var date1 = now.AddMonths(1);
                int day2 = 0;
                for (int i = x; i < 7; i++)
                {
                    int day3 = day2 + 1;
                    calendaris.Add(new Calendari(2, x, y, day3.ToString(), new DateTime(date1.Year, date1.Month, day3), minDate, maxDate));
                    x++; day2++;
                }
                if (y < 5)
                {
                    y++;
                    for (int i = 0; i < 7; i++)
                    {
                        int day3 = day2 + 1;
                        calendaris.Add(new Calendari(2, i, y, day3.ToString(), new DateTime(date1.Year, date1.Month, day3), minDate, maxDate));
                        day2++;
                    }
                }
            }
            return calendaris;
        }

        #endregion

        #endregion

        #region 鼠标

        ITaskOpacity hover_lefts, hover_left, hover_rights, hover_right, hover_year, hover_month, hover_year_r, hover_month_r;
        Rectangle rect_lefts = new Rectangle(-20, -20, 10, 10), rect_left = new Rectangle(-20, -20, 10, 10);
        Rectangle rect_rights = new Rectangle(-20, -20, 10, 10), rect_right = new Rectangle(-20, -20, 10, 10);
        Rectangle rect_year = new Rectangle(-20, -20, 10, 10), rect_year2 = new Rectangle(-20, -20, 10, 10), rect_month = new Rectangle(-20, -20, 10, 10);
        Rectangle rect_year_r = new Rectangle(-20, -20, 10, 10), rect_month_r = new Rectangle(-20, -20, 10, 10);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (left_buttons != null && rect_read_left.Contains(e.X, e.Y)) if (!scrollY_left.MouseDown(e.Location)) return;
        }

        bool DisableMouse = true;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DisableMouse) return;
            if (scrollY_left.MouseMove(e.Location))
            {
                int count = 0, hand = 0;
                bool _hover_lefts = rect_lefts.Contains(e.X, e.Y),
                 _hover_rights = rect_rights.Contains(e.X, e.Y),
                 _hover_left = (showType == 0 && rect_left.Contains(e.X, e.Y)),
                 _hover_right = (showType == 0 && rect_right.Contains(e.X, e.Y));

                bool _hover_year = false, _hover_month = false, _hover_year_r = false, _hover_month_r = false;
                if (showType != 2)
                {
                    _hover_year = showType == 0 ? rect_year.Contains(e.X, e.Y) : rect_year2.Contains(e.X, e.Y);
                    _hover_month = rect_month.Contains(e.X, e.Y);
                    _hover_year_r = rect_year_r.Contains(e.X, e.Y);
                    _hover_month_r = rect_month_r.Contains(e.X, e.Y);
                }

                if (_hover_lefts != hover_lefts.Switch) count++;
                if (_hover_left != hover_left.Switch) count++;
                if (_hover_rights != hover_rights.Switch) count++;
                if (_hover_right != hover_right.Switch) count++;

                if (_hover_year != hover_year.Switch) count++;
                if (_hover_month != hover_month.Switch) count++;
                if (_hover_year_r != hover_year_r.Switch) count++;
                if (_hover_month_r != hover_month_r.Switch) count++;

                hover_lefts.Switch = _hover_lefts;
                hover_left.Switch = _hover_left;
                hover_rights.Switch = _hover_rights;
                hover_right.Switch = _hover_right;
                hover_year.Switch = _hover_year;
                hover_month.Switch = _hover_month;
                hover_year_r.Switch = _hover_year_r;
                hover_month_r.Switch = _hover_month_r;
                if (hover_lefts.Switch || hover_left.Switch || hover_rights.Switch || hover_right.Switch || hover_year.Switch || hover_month.Switch || hover_year_r.Switch || hover_month_r.Switch) hand++;
                else
                {
                    if (showType == 1)
                    {
                        if (calendar_month != null)
                        {
                            foreach (var it in calendar_month)
                            {
                                bool hove = it.enable && it.rect.Contains(e.X, e.Y);
                                if (it.hover != hove) count++;
                                it.hover = hove;
                                if (it.hover) hand++;
                            }
                        }
                    }
                    else if (showType == 2)
                    {
                        if (calendar_year != null)
                        {
                            foreach (var it in calendar_year)
                            {
                                bool hove = it.enable && it.rect.Contains(e.X, e.Y);
                                if (it.hover != hove) count++;
                                it.hover = hove;
                                if (it.hover) hand++;
                            }
                        }
                    }
                    else
                    {
                        if (calendar_day != null)
                        {
                            foreach (var it in calendar_day)
                            {
                                bool hove = it.enable && it.rect.Contains(e.X, e.Y);
                                if (it.hover != hove) count++;
                                it.hover = hove;
                                if (it.hover)
                                {
                                    if (isEnd) oldTimeHover = it.date;
                                    hand++;
                                }
                            }
                        }
                        if (calendar_day2 != null)
                        {
                            foreach (var it in calendar_day2)
                            {
                                bool hove = it.enable && it.rect.Contains(e.X, e.Y);
                                if (it.hover != hove) count++;
                                it.hover = hove;
                                if (it.hover)
                                {
                                    if (isEnd) oldTimeHover = it.date;
                                    hand++;
                                }
                            }
                        }
                        if (left_buttons != null)
                        {
                            foreach (var it in left_buttons)
                            {
                                if (it.Contains(e.Location, 0, scrollY_left.Value, out var change)) hand++;
                                if (change) count++;
                            }
                        }
                    }
                }
                if (count > 0) Print();
                SetCursor(hand > 0);
            }
            else SetCursor(false);
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            scrollY_left.Leave();
            hover_lefts.Switch = false;
            hover_left.Switch = false;
            hover_rights.Switch = false;
            hover_right.Switch = false;
            hover_year.Switch = false;
            hover_month.Switch = false;
            hover_year_r.Switch = false;
            hover_month_r.Switch = false;
            if (calendar_year != null)
            {
                foreach (var it in calendar_year)
                {
                    it.hover = false;
                }
            }
            if (calendar_month != null)
            {
                foreach (var it in calendar_month)
                {
                    it.hover = false;
                }
            }
            if (calendar_day != null)
            {
                foreach (var it in calendar_day)
                {
                    it.hover = false;
                }
            }
            if (calendar_day2 != null)
            {
                foreach (var it in calendar_day2)
                {
                    it.hover = false;
                }
            }
            SetCursor(false);
            Print();
            base.OnMouseLeave(e);
        }

        int showType = 0;
        void CSize()
        {
            if (left_buttons != null) t_x = showType == 0 ? left_button : 0;

            int r_h;
            if (showType == 0)
            {
                t_width = t_x + t_one_width * 2;
                if (calendar_day == null) r_h = 348 + 20;
                else r_h = t_top * 2 + (12 * 2) + (int)Math.Ceiling((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16) / 7F) + 20;
            }
            else
            {
                t_width = t_x + t_one_width;
                if (calendar_day == null) r_h = 348 + 20;
                else r_h = t_top * 2 + (12 * 2) + (int)Math.Ceiling((calendar_day[calendar_day.Count - 1].y + 2) * (t_one_width - 16) / 7F) + 20;
            }
            SetSize(t_width + 20, r_h);

            if (showType == 0)
            {
                rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
                rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
                rect_rights = new Rectangle(t_width + 10 - t_top, 10, t_top, t_top);
                rect_right = new Rectangle(t_width + 10 - t_top * 2, 10, t_top, t_top);

                rect_year = new Rectangle(t_x + 10 + t_one_width / 2 - year_width, 10, year_width, t_top);
                rect_year2 = new Rectangle(t_x + 10 + (t_one_width - year2_width) / 2, 10, year2_width, t_top);
                rect_month = new Rectangle(t_x + 10 + t_one_width / 2, 10, month_width, t_top);

                rect_year_r = new Rectangle(rect_year.Left + t_one_width, rect_year.Y, rect_year.Width, rect_year.Height);
                rect_month_r = new Rectangle(rect_month.Left + t_one_width, rect_month.Y, rect_month.Width, rect_month.Height);
            }
            else
            {
                rect_lefts = new Rectangle(t_x + 10, 10, t_top, t_top);
                rect_left = new Rectangle(t_x + 10 + t_top, 10, t_top, t_top);
                rect_rights = new Rectangle(t_one_width + 10 - t_top, 10, t_top, t_top);
                rect_right = new Rectangle(t_one_width + 10 - t_top * 2, 10, t_top, t_top);

                rect_year = new Rectangle(t_x + 10 + t_one_width / 2 - year_width, 10, year_width, t_top);
                rect_year2 = new Rectangle(t_x + 10 + (t_one_width - year2_width) / 2, 10, year2_width, t_top);
                rect_month = new Rectangle(t_x + 10 + t_one_width / 2, 10, month_width, t_top);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            scrollY_left.MouseUp(e.Location);
            if (e.Button == MouseButtons.Left)
            {
                if (rect_lefts.Contains(e.X, e.Y))
                {
                    if (hover_lefts.Enable)
                    {
                        if (showType == 2) Date = _Date.AddYears(-10);
                        else Date = _Date.AddYears(-1);
                        Print();
                    }
                    return;
                }
                else if (rect_rights.Contains(e.X, e.Y))
                {
                    if (hover_rights.Enable)
                    {
                        if (showType == 2) Date = _Date.AddYears(10);
                        else Date = _Date.AddYears(1);
                        Print();
                    }
                    return;
                }
                else if (showType == 0 && rect_left.Contains(e.X, e.Y))
                {
                    if (hover_left.Enable)
                    {
                        Date = _Date.AddMonths(-1);
                        Print();
                    }
                    return;
                }
                else if (showType == 0 && rect_right.Contains(e.X, e.Y))
                {
                    if (hover_right.Enable)
                    {
                        Date = _Date.AddMonths(1);
                        Print();
                    }
                    return;
                }
                else if ((showType == 0 && (rect_year.Contains(e.X, e.Y) || rect_year_r.Contains(e.X, e.Y))) || (showType != 0 && rect_year2.Contains(e.X, e.Y)))
                {
                    showType = 2;
                    CSize();
                    Print();
                    return;
                }
                else if (rect_month.Contains(e.X, e.Y) || rect_month_r.Contains(e.X, e.Y))
                {
                    showType = 1;
                    CSize();
                    Print();
                    return;
                }
                else
                {
                    if (showType == 1)
                    {
                        if (calendar_month != null)
                        {
                            foreach (var it in calendar_month)
                            {
                                if (it.enable && it.rect.Contains(e.X, e.Y))
                                {
                                    Date = it.date;
                                    showType = 0;
                                    CSize();
                                    Print();
                                    return;
                                }
                            }
                        }
                    }
                    else if (showType == 2)
                    {
                        if (calendar_year != null)
                        {
                            foreach (var it in calendar_year)
                            {
                                if (it.enable && it.rect.Contains(e.X, e.Y))
                                {
                                    Date = it.date;
                                    showType = 1;
                                    CSize();
                                    Print();
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (calendar_day != null)
                        {
                            foreach (var it in calendar_day)
                            {
                                if (it.enable && it.rect.Contains(e.X, e.Y))
                                {
                                    if (SetDate(it)) return;
                                    IClose();
                                    return;
                                }
                            }
                        }
                        if (calendar_day2 != null)
                        {
                            foreach (var it in calendar_day2)
                            {
                                if (it.enable && it.rect.Contains(e.X, e.Y))
                                {
                                    if (SetDate(it)) return;
                                    IClose();
                                    return;
                                }
                            }
                        }
                        if (left_buttons != null)
                        {
                            foreach (var it in left_buttons)
                            {
                                if (it.Contains(e.Location, 0, scrollY_left.Value, out _))
                                {
                                    action_btns(it.Tag);
                                    IClose();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            base.OnMouseUp(e);
        }

        bool isEnd = false;
        DateTime? oldTime, oldTimeHover;
        bool SetDate(Calendari item)
        {
            if (isEnd && oldTime.HasValue)
            {
                SetDateE(oldTime.Value, item.date);
                return false;
            }
            SetDateS(item.date);
            Print();
            return true;
        }

        public void SetDateS(DateTime date)
        {
            SelDate = null;
            oldTimeHover = oldTime = date;
            isEnd = true;
        }

        public void SetDateE(DateTime sdate, DateTime edate)
        {
            if (sdate == edate) SelDate = new DateTime[] { edate, edate };
            else if (sdate < edate) SelDate = new DateTime[] { sdate, edate };
            else SelDate = new DateTime[] { edate, sdate };
            action(SelDate);
            isEnd = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (left_buttons != null && rect_read_left.Contains(e.X, e.Y))
                {
                    scrollY_left.MouseWheel(e.Delta);
                    Print();
                    base.OnMouseWheel(e);
                    return;
                }
                MouseWheelDay(e);
            }
            base.OnMouseWheel(e);
        }

        void MouseWheelDay(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (showType == 1)
                {
                    if (hover_lefts.Enable) Date = _Date.AddYears(-1);
                    else return;
                }
                else if (showType == 2)
                {
                    if (hover_lefts.Enable) Date = _Date.AddYears(-10);
                    else return;
                }
                else
                {
                    if (hover_left.Enable) Date = _Date.AddMonths(-1);
                    else return;
                }
                Print();
            }
            else
            {
                if (showType == 1)
                {
                    if (hover_rights.Enable) Date = _Date.AddYears(1);
                    else return;
                }
                else if (showType == 2)
                {
                    if (hover_rights.Enable) Date = _Date.AddYears(10);
                    else return;
                }
                else
                {
                    if (hover_right.Enable) Date = _Date.AddMonths(1);
                    else return;
                }
                Print();
            }
        }

        #endregion

        bool init = false;
        public override void LoadOK()
        {
            DisableMouse = false;
            init = true;
            Print();
            CanLoadMessage = true;
            LoadMessage();
        }

        float AnimationBarValue = 0;
        public void SetArrow(float x)
        {
            if (AnimationBarValue == x) return;
            AnimationBarValue = x;
            if (init) Print();
            else DisposeTmp();
        }

        #region 渲染

        StringFormat s_f = Helper.SF();
        StringFormat s_f_L = Helper.SF(lr: StringAlignment.Far);
        StringFormat s_f_LE = Helper.SF_Ellipsis(lr: StringAlignment.Near);
        StringFormat s_f_R = Helper.SF(lr: StringAlignment.Near);
        public override Bitmap PrintBit()
        {
            var rect = TargetRectXY;
            var rect_read = new Rectangle(10, 10, rect.Width - 20, rect.Height - 20);
            Bitmap original_bmp = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(original_bmp).High())
            {
                using (var path = rect_read.RoundPath(Radius))
                {
                    DrawShadow(g, rect);
                    using (var brush = new SolidBrush(Style.Db.BgElevated))
                    {
                        g.FillPath(brush, path);
                        if (ArrowAlign != TAlign.None)
                        {
                            if (AnimationBarValue != 0F)
                            {
                                g.FillPolygon(brush, ArrowAlign.AlignLines(ArrowSize, rect, new RectangleF(rect_read.X + AnimationBarValue, rect_read.Y, rect_read.Width, rect_read.Height)));
                            }
                            else g.FillPolygon(brush, ArrowAlign.AlignLines(ArrowSize, rect, rect_read));
                        }
                    }
                }

                #region 方向

                using (var pen_arrow = new Pen(Style.Db.TextTertiary, 1.6F * Config.Dpi))
                using (var pen_arrow_hover = new Pen(Style.Db.Text, pen_arrow.Width))
                using (var pen_arrow_enable = new Pen(Style.Db.FillSecondary, pen_arrow.Width))
                {
                    if (hover_lefts.Animation)
                    {
                        PointF[] tl1 = TAlignMini.Left.TriangleLines(new RectangleF(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26F),
                            tl2 = TAlignMini.Left.TriangleLines(new RectangleF(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26F);
                        g.DrawLines(pen_arrow, tl1);
                        g.DrawLines(pen_arrow, tl2);
                        using (var pen_arrow_hovers = new Pen(Helper.ToColor(hover_lefts.Value, pen_arrow_hover.Color), pen_arrow_hover.Width))
                        {
                            g.DrawLines(pen_arrow_hovers, tl1);
                            g.DrawLines(pen_arrow_hovers, tl2);
                        }
                    }
                    else if (hover_lefts.Switch)
                    {
                        g.DrawLines(pen_arrow_hover, TAlignMini.Left.TriangleLines(new RectangleF(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26F));
                        g.DrawLines(pen_arrow_hover, TAlignMini.Left.TriangleLines(new RectangleF(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), 0.26F));
                    }
                    else if (hover_lefts.Enable)
                    {
                        g.DrawLines(pen_arrow, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), .26F));
                        g.DrawLines(pen_arrow, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), .26F));
                    }
                    else
                    {
                        g.DrawLines(pen_arrow_enable, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X - 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), .26F));
                        g.DrawLines(pen_arrow_enable, TAlignMini.Left.TriangleLines(new Rectangle(rect_lefts.X + 4, rect_lefts.Y, rect_lefts.Width, rect_lefts.Height), .26F));
                    }

                    if (hover_rights.Animation)
                    {
                        PointF[] tl1 = TAlignMini.Right.TriangleLines(new RectangleF(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26F),
                            tl2 = TAlignMini.Right.TriangleLines(new RectangleF(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), 0.26F);
                        g.DrawLines(pen_arrow, tl1);
                        g.DrawLines(pen_arrow, tl2);
                        using (var pen_arrow_hovers = new Pen(Helper.ToColor(hover_rights.Value, pen_arrow_hover.Color), pen_arrow_hover.Width))
                        {
                            g.DrawLines(pen_arrow_hovers, tl1);
                            g.DrawLines(pen_arrow_hovers, tl2);
                        }
                    }
                    else if (hover_rights.Switch)
                    {
                        g.DrawLines(pen_arrow_hover, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), .26F));
                        g.DrawLines(pen_arrow_hover, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), .26F));
                    }
                    else if (hover_rights.Enable)
                    {
                        g.DrawLines(pen_arrow, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), .26F));
                        g.DrawLines(pen_arrow, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), .26F));
                    }
                    else
                    {
                        g.DrawLines(pen_arrow_enable, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X - 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), .26F));
                        g.DrawLines(pen_arrow_enable, TAlignMini.Right.TriangleLines(new Rectangle(rect_rights.X + 4, rect_rights.Y, rect_rights.Width, rect_rights.Height), .26F));
                    }

                    if (showType == 0)
                    {
                        if (hover_left.Animation)
                        {
                            var tl = TAlignMini.Left.TriangleLines(rect_left, 0.26F);
                            g.DrawLines(pen_arrow, tl);
                            using (var pen_arrow_hovers = new Pen(Helper.ToColor(hover_left.Value, pen_arrow_hover.Color), pen_arrow_hover.Width))
                            {
                                g.DrawLines(pen_arrow_hovers, tl);
                            }
                        }
                        else if (hover_left.Switch) g.DrawLines(pen_arrow_hover, TAlignMini.Left.TriangleLines(rect_left, .26F));
                        else if (hover_left.Enable) g.DrawLines(pen_arrow, TAlignMini.Left.TriangleLines(rect_left, .26F));
                        else g.DrawLines(pen_arrow_enable, TAlignMini.Left.TriangleLines(rect_left, .26F));

                        if (hover_right.Animation)
                        {
                            var tl = TAlignMini.Right.TriangleLines(rect_right, 0.26F);
                            g.DrawLines(pen_arrow, tl);
                            using (var pen_arrow_hovers = new Pen(Helper.ToColor(hover_right.Value, pen_arrow_hover.Color), pen_arrow_hover.Width))
                            {
                                g.DrawLines(pen_arrow_hovers, tl);
                            }
                        }
                        else if (hover_right.Switch) g.DrawLines(pen_arrow_hover, TAlignMini.Right.TriangleLines(rect_right, .26F));
                        else if (hover_right.Enable) g.DrawLines(pen_arrow, TAlignMini.Right.TriangleLines(rect_right, .26F));
                        else g.DrawLines(pen_arrow_enable, TAlignMini.Right.TriangleLines(rect_right, .26F));
                    }
                }

                #endregion

                if (showType == 1 && calendar_month != null) PrintMonth(g, rect_read, calendar_month);
                else if (showType == 2 && calendar_year != null) PrintYear(g, rect_read, calendar_year);
                else if (calendar_day != null && calendar_day2 != null) PrintDay(g, rect_read, calendar_day, calendar_day2);
            }
            return original_bmp;
        }

        #region 渲染帮助

        #region 年模式

        /// <summary>
        /// 渲染年模式
        /// </summary>
        /// <param name="g">GDI</param>
        /// <param name="rect_read">真实区域</param>
        /// <param name="datas">数据</param>
        void PrintYear(Graphics g, Rectangle rect_read, List<Calendari> datas)
        {
            using (var brush_fore_disable = new SolidBrush(Style.Db.TextQuaternary))
            using (var brush_bg_disable = new SolidBrush(Style.Db.FillTertiary))
            using (var brush_fore = new SolidBrush(Style.Db.TextBase))
            {
                using (var font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold))
                {
                    RectangleF rect_l = new RectangleF(rect_read.X, rect_read.Y, rect_read.Width, t_top);

                    if (hover_year.Animation)
                    {
                        g.DrawStr(year_str, font, brush_fore, rect_l, s_f);
                        using (var brush_hove = new SolidBrush(Helper.ToColor(hover_year.Value, Style.Db.Primary)))
                        {
                            g.DrawStr(year_str, font, brush_hove, rect_l, s_f);
                        }
                    }
                    else if (hover_year.Switch)
                    {
                        using (var brush_hove = new SolidBrush(Style.Db.Primary))
                        {
                            g.DrawStr(year_str, font, brush_hove, rect_l, s_f);
                        }
                    }
                    else g.DrawStr(year_str, font, brush_fore, rect_l, s_f);
                }

                int size_w = (rect_read.Width - 16) / 3, size_h = (rect_read.Width - 16) / 7 * 2;
                int y = rect_read.Y + t_top;
                if (size_year)
                {
                    size_year = false;
                    foreach (var it in datas)
                    {
                        it.rect = new Rectangle(rect_read.X + 8 + (size_w * it.x), y + (size_h * it.y), size_w, size_h);
                    }
                }
                foreach (var it in datas)
                {
                    using (var path = it.rect_read.RoundPath(Radius))
                    {
                        if (SelDate != null && (SelDate[0].ToString("yyyy") == it.date_str || (SelDate.Length > 1 && SelDate[1].ToString("yyyy") == it.date_str)))
                        {
                            using (var brush_hove = new SolidBrush(Style.Db.Primary))
                            {
                                g.FillPath(brush_hove, path);
                            }

                            using (var brush_active_fore = new SolidBrush(Style.Db.PrimaryColor))
                            {
                                g.DrawStr(it.v, Font, brush_active_fore, it.rect, s_f);
                            }
                        }
                        else if (it.enable)
                        {
                            if (it.hover)
                            {
                                using (var brush_hove = new SolidBrush(Style.Db.FillTertiary))
                                {
                                    g.FillPath(brush_hove, path);
                                }
                            }
                            if (DateNow.ToString("yyyy-MM-dd") == it.date_str)
                            {
                                using (var brush_hove = new Pen(Style.Db.Primary, Config.Dpi))
                                {
                                    g.DrawPath(brush_hove, path);
                                }
                            }
                            g.DrawStr(it.v, Font, it.t == 1 ? brush_fore : brush_fore_disable, it.rect, s_f);
                        }
                        else
                        {
                            g.FillRectangle(brush_bg_disable, new Rectangle(it.rect.X, it.rect_read.Y, it.rect.Width, it.rect_read.Height));
                            if (DateNow.ToString("yyyy-MM-dd") == it.date_str)
                            {
                                using (var brush_hove = new Pen(Style.Db.Primary, Config.Dpi))
                                {
                                    g.DrawPath(brush_hove, path);
                                }
                            }
                            g.DrawStr(it.v, Font, brush_fore_disable, it.rect, s_f);
                        }
                    }
                }
            }
        }

        #endregion

        #region 月模式

        /// <summary>
        /// 渲染月模式
        /// </summary>
        /// <param name="g">GDI</param>
        /// <param name="rect_read">真实区域</param>
        /// <param name="datas">数据</param>
        void PrintMonth(Graphics g, Rectangle rect_read, List<Calendari> datas)
        {
            using (var brush_fore_disable = new SolidBrush(Style.Db.TextQuaternary))
            using (var brush_bg_disable = new SolidBrush(Style.Db.FillTertiary))
            using (var brush_fore = new SolidBrush(Style.Db.TextBase))
            {
                using (var font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold))
                {
                    var rect_l = new RectangleF(rect_read.X, rect_read.Y, rect_read.Width, t_top);

                    if (hover_year.Animation)
                    {
                        g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_fore, rect_l, s_f);
                        using (var brush_hove = new SolidBrush(Helper.ToColor(hover_year.Value, Style.Db.Primary)))
                        {
                            g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_hove, rect_l, s_f);
                        }
                    }
                    else if (hover_year.Switch)
                    {
                        using (var brush_hove = new SolidBrush(Style.Db.Primary))
                        {
                            g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_hove, rect_l, s_f);
                        }
                    }
                    else g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_fore, rect_l, s_f);
                }

                int size_w = (rect_read.Width - 16) / 3, size_h = (rect_read.Width - 16) / 7 * 2;
                int y = rect_read.Y + t_top;
                if (size_month)
                {
                    size_month = false;
                    foreach (var it in datas)
                    {
                        it.rect = new Rectangle(rect_read.X + 8 + (size_w * it.x), y + (size_h * it.y), size_w, size_h);
                    }
                }
                foreach (var it in datas)
                {
                    using (var path = it.rect_read.RoundPath(Radius))
                    {
                        if (SelDate != null && (SelDate[0].ToString("yyyy-MM") == it.date_str || (SelDate.Length > 1 && SelDate[1].ToString("yyyy-MM") == it.date_str)))
                        {
                            using (var brush_hove = new SolidBrush(Style.Db.Primary))
                            {
                                g.FillPath(brush_hove, path);
                            }

                            using (var brush_active_fore = new SolidBrush(Style.Db.PrimaryColor))
                            {
                                g.DrawStr(it.v, Font, brush_active_fore, it.rect, s_f);
                            }
                        }
                        else if (it.enable)
                        {
                            if (it.hover)
                            {
                                using (var brush_hove = new SolidBrush(Style.Db.FillTertiary))
                                {
                                    g.FillPath(brush_hove, path);
                                }
                            }
                            if (DateNow.ToString("yyyy-MM-dd") == it.date_str)
                            {
                                using (var brush_hove = new Pen(Style.Db.Primary, Config.Dpi))
                                {
                                    g.DrawPath(brush_hove, path);
                                }
                            }
                            g.DrawStr(it.v, Font, brush_fore, it.rect, s_f);
                        }
                        else
                        {
                            g.FillRectangle(brush_bg_disable, new Rectangle(it.rect.X, it.rect_read.Y, it.rect.Width, it.rect_read.Height));
                            if (DateNow.ToString("yyyy-MM-dd") == it.date_str)
                            {
                                using (var brush_hove = new Pen(Style.Db.Primary, Config.Dpi))
                                {
                                    g.DrawPath(brush_hove, path);
                                }
                            }
                            g.DrawStr(it.v, Font, brush_fore_disable, it.rect, s_f);
                        }
                    }
                }
            }
        }

        #endregion

        #region 天模式

        Rectangle rect_read_left;
        /// <summary>
        /// 渲染天模式
        /// </summary>
        /// <param name="g">GDI</param>
        /// <param name="rect_read">真实区域</param>
        /// <param name="datas">数据</param>
        void PrintDay(Graphics g, Rectangle rect_read, List<Calendari> datas, List<Calendari> datas2)
        {
            using (var brush_fore = new SolidBrush(Style.Db.TextBase))
            {
                float xm = t_one_width / 2F;
                using (var font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold))
                {
                    RectangleF rect_l = new RectangleF(t_x + rect_read.X, rect_read.Y, xm, t_top), rect_r = new RectangleF(t_x + rect_read.X + xm, rect_read.Y, xm, t_top);

                    if (hover_year.Animation)
                    {
                        g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_fore, rect_l, s_f_L);
                        using (var brush_hove = new SolidBrush(Helper.ToColor(hover_year.Value, Style.Db.Primary)))
                        {
                            g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_hove, rect_l, s_f_L);
                        }
                    }
                    else if (hover_year.Switch)
                    {
                        using (var brush_hove = new SolidBrush(Style.Db.Primary))
                        {
                            g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_hove, rect_l, s_f_L);
                        }
                    }
                    else g.DrawStr(_Date.ToString("yyyy") + YearButton, font, brush_fore, rect_l, s_f_L);

                    if (hover_month.Animation)
                    {
                        g.DrawStr(_Date.ToString("MM") + MonthButton, font, brush_fore, rect_r, s_f_R);
                        using (var brush_hove = new SolidBrush(Helper.ToColor(hover_month.Value, Style.Db.Primary)))
                        {
                            g.DrawStr(_Date.ToString("MM") + MonthButton, font, brush_hove, rect_r, s_f_R);
                        }
                    }
                    else if (hover_month.Switch)
                    {
                        using (var brush_hove = new SolidBrush(Style.Db.Primary))
                        {
                            g.DrawStr(_Date.ToString("MM") + MonthButton, font, brush_hove, rect_r, s_f_R);
                        }
                    }
                    else g.DrawStr(_Date.ToString("MM") + MonthButton, font, brush_fore, rect_r, s_f_R);

                    #region 右

                    RectangleF rect_r_l = new RectangleF(rect_l.X + t_one_width, rect_l.Y, rect_l.Width, rect_l.Height), rect_r_r = new RectangleF(rect_r.X + t_one_width, rect_r.Y, rect_r.Width, rect_r.Height);
                    if (hover_year_r.Animation)
                    {
                        g.DrawStr(_Date_R.ToString("yyyy") + YearButton, font, brush_fore, rect_r_l, s_f_L);
                        using (var brush_hove = new SolidBrush(Helper.ToColor(hover_year_r.Value, Style.Db.Primary)))
                        {
                            g.DrawStr(_Date_R.ToString("yyyy") + YearButton, font, brush_hove, rect_r_l, s_f_L);
                        }
                    }
                    else if (hover_year_r.Switch)
                    {
                        using (var brush_hove = new SolidBrush(Style.Db.Primary))
                        {
                            g.DrawStr(_Date_R.ToString("yyyy") + YearButton, font, brush_hove, rect_r_l, s_f_L);
                        }
                    }
                    else g.DrawStr(_Date_R.ToString("yyyy") + YearButton, font, brush_fore, rect_r_l, s_f_L);

                    if (hover_month_r.Animation)
                    {
                        g.DrawStr(_Date_R.ToString("MM") + MonthButton, font, brush_fore, rect_r_r, s_f_R);
                        using (var brush_hove = new SolidBrush(Helper.ToColor(hover_month_r.Value, Style.Db.Primary)))
                        {
                            g.DrawStr(_Date_R.ToString("MM") + MonthButton, font, brush_hove, rect_r_r, s_f_R);
                        }
                    }
                    else if (hover_month_r.Switch)
                    {
                        using (var brush_hove = new SolidBrush(Style.Db.Primary))
                        {
                            g.DrawStr(_Date_R.ToString("MM") + MonthButton, font, brush_hove, rect_r_r, s_f_R);
                        }
                    }
                    else g.DrawStr(_Date_R.ToString("MM") + MonthButton, font, brush_fore, rect_r_r, s_f_R);

                    #endregion
                }

                using (var brush_split = new SolidBrush(Style.Db.Split))
                {
                    g.FillRectangle(brush_split, new RectangleF(t_x + rect_read.X, rect_read.Y + t_top, t_width - t_x, 1F));
                    if (left_buttons != null) g.FillRectangle(brush_split, new RectangleF(t_x + rect_read.X, rect_read.Y, 1F, rect_read.Height));
                }
                int y = rect_read.Y + t_top + 12;
                int size = (t_one_width - 16) / 7;
                using (var brush = new SolidBrush(Style.Db.Text))
                {
                    float x = t_x + rect_read.X + 8F, x2 = t_x + rect_read.X + t_one_width + 8F;
                    g.DrawStr(MondayButton, Font, brush, new RectangleF(x, y, size, size), s_f);
                    g.DrawStr(TuesdayButton, Font, brush, new RectangleF(x + size, y, size, size), s_f);
                    g.DrawStr(WednesdayButton, Font, brush, new RectangleF(x + size * 2F, y, size, size), s_f);
                    g.DrawStr(ThursdayButton, Font, brush, new RectangleF(x + size * 3F, y, size, size), s_f);
                    g.DrawStr(FridayButton, Font, brush, new RectangleF(x + size * 4F, y, size, size), s_f);
                    g.DrawStr(SaturdayButton, Font, brush, new RectangleF(x + size * 5F, y, size, size), s_f);
                    g.DrawStr(SundayButton, Font, brush, new RectangleF(x + size * 6F, y, size, size), s_f);

                    g.DrawStr(MondayButton, Font, brush, new RectangleF(x2, y, size, size), s_f);
                    g.DrawStr(TuesdayButton, Font, brush, new RectangleF(x2 + size, y, size, size), s_f);
                    g.DrawStr(WednesdayButton, Font, brush, new RectangleF(x2 + size * 2F, y, size, size), s_f);
                    g.DrawStr(ThursdayButton, Font, brush, new RectangleF(x2 + size * 3F, y, size, size), s_f);
                    g.DrawStr(FridayButton, Font, brush, new RectangleF(x2 + size * 4F, y, size, size), s_f);
                    g.DrawStr(SaturdayButton, Font, brush, new RectangleF(x2 + size * 5F, y, size, size), s_f);
                    g.DrawStr(SundayButton, Font, brush, new RectangleF(x2 + size * 6F, y, size, size), s_f);
                }

                y += size;
                if (sizeday)
                {
                    sizeday = false;
                    int size_one = (int)(size * .666F);
                    foreach (var it in datas)
                    {
                        it.SetRect(new Rectangle(t_x + rect_read.X + 8 + (size * it.x), y + (size * it.y), size, size), size_one);
                    }
                    foreach (var it in datas2)
                    {
                        it.SetRect(new Rectangle(t_x + rect_read.X + t_one_width + 8 + (size * it.x), y + (size * it.y), size, size), size_one);
                    }

                    if (left_buttons != null)
                    {
                        int btn_one = (int)(left_button * .9F), btn_height_one = (int)(t_time_height * .93F), btn_one2 = (int)(left_button * .8F);

                        rect_read_left = new Rectangle(rect_read.X, rect_read.Y, t_x, t_h - rect_read.Y * 2);

                        scrollY_left.SizeChange(new Rectangle(rect_read.X, rect_read.Y + 8, t_x, t_h - (8 + rect_read.Y) * 2));
                        scrollY_left.SetVrSize(t_time_height * left_buttons.Count, t_h - 20 - rect_read.Y * 2);

                        int _x = (left_button - btn_one) / 2, _x2 = (btn_one - btn_one2) / 2, _y = rect_read.Y + (t_time_height - btn_height_one) / 2;
                        foreach (var it in left_buttons)
                        {
                            var rect_n = new Rectangle(0, t_time_height * it.y, left_button, t_time_height);
                            it.rect_read = new Rectangle(rect_n.X + _x, rect_n.Y + _y, btn_one, btn_height_one);
                            it.rect = new Rectangle(rect_read.X + rect_n.X, rect_read.Y + rect_n.Y, rect_n.Width, rect_n.Height);

                            it.rect_text = new Rectangle(rect_read.X + _x2, it.rect_read.Y, btn_one2, it.rect_read.Height);
                        }
                    }
                }
                using (var brush_fore_disable = new SolidBrush(Style.Db.TextQuaternary))
                using (var brush_bg_disable = new SolidBrush(Style.Db.FillTertiary))
                using (var brush_bg_active = new SolidBrush(Style.Db.Primary))
                using (var brush_bg_activebg = new SolidBrush(Style.Db.PrimaryBg))
                using (var brush_fore_active = new SolidBrush(Style.Db.PrimaryColor))
                {
                    if (oldTimeHover.HasValue && oldTime.HasValue)
                    {
                        if (oldTimeHover.Value != oldTime.Value && oldTimeHover.Value > oldTime.Value)
                        {
                            PrintCalendarMutual(g, oldTime.Value, oldTimeHover.Value, brush_bg_active, brush_bg_activebg, datas);
                            PrintCalendarMutual(g, oldTime.Value, oldTimeHover.Value, brush_bg_active, brush_bg_activebg, datas2);
                        }
                        else
                        {
                            foreach (var it in datas)
                            {
                                if (it.t == 1 && it.date == oldTime.Value)
                                {
                                    using (var path_l = it.rect_read.RoundPath(Radius, true, false, false, true))
                                    {
                                        g.FillPath(brush_bg_active, path_l);
                                    }
                                }
                            }
                            foreach (var it in datas2)
                            {
                                if (it.t == 1 && it.date == oldTime.Value)
                                {
                                    using (var path_l = it.rect_read.RoundPath(Radius, true, false, false, true))
                                    {
                                        g.FillPath(brush_bg_active, path_l);
                                    }
                                }
                            }
                        }
                    }

                    PrintCalendar(g, brush_fore, brush_fore_disable, brush_bg_disable, brush_bg_active, brush_bg_activebg, brush_fore_active, datas);
                    PrintCalendar(g, brush_fore, brush_fore_disable, brush_bg_disable, brush_bg_active, brush_bg_activebg, brush_fore_active, datas2);

                    if (rect_read.Height > t_time_height)
                    {
                        if (left_buttons != null)
                        {
                            var state = g.Save();
                            g.SetClip(new Rectangle(rect_read.X, rect_read.Y, left_button, rect_read.Height));
                            g.TranslateTransform(rect_read.X, rect_read.Y - scrollY_left.Value);
                            foreach (var it in left_buttons)
                            {
                                using (var path = it.rect_read.RoundPath(Radius))
                                {
                                    if (it.hover)
                                    {
                                        using (var brush_hove = new SolidBrush(Style.Db.FillTertiary))
                                        {
                                            g.FillPath(brush_hove, path);
                                        }
                                    }
                                    g.DrawStr(it.v, Font, brush_fore, it.rect_text, s_f_LE);
                                }
                            }
                            g.Restore(state);
                            scrollY_left.Paint(g);
                        }
                    }
                }
            }
        }
        void PrintCalendarMutual(Graphics g, DateTime oldTime, DateTime oldTimeHover, Brush brush_bg_active, Brush brush_bg_activebg, List<Calendari> datas)
        {
            foreach (var it in datas)
            {
                if (it.t == 1)
                {
                    if (it.date > oldTime && it.date < oldTimeHover)
                    {
                        g.FillRectangle(brush_bg_activebg, new RectangleF(it.rect.X - 1F, it.rect_read.Y, it.rect.Width + 2F, it.rect_read.Height));
                    }
                    else if (it.date == oldTime)
                    {
                        g.FillRectangle(brush_bg_activebg, new RectangleF(it.rect_read.Right, it.rect_read.Y, it.rect.Width - it.rect_read.Width, it.rect_read.Height));
                        using (var path_l = it.rect_read.RoundPath(Radius, true, false, false, true))
                        {
                            g.FillPath(brush_bg_active, path_l);
                        }
                    }
                    else if (it.date == oldTimeHover)
                    {
                        g.FillRectangle(brush_bg_activebg, new RectangleF(it.rect.X, it.rect_read.Y, it.rect_read.Width, it.rect_read.Height));
                        using (var path_r = it.rect_read.RoundPath(Radius, false, true, true, false))
                        {
                            g.FillPath(brush_bg_active, path_r);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 渲染日期面板
        /// </summary>
        /// <param name="g">GDI</param>
        /// <param name="brush_fore">文字颜色</param>
        /// <param name="brush_fore_disable">文字禁用颜色</param>
        /// <param name="brush_bg_disable">背景禁用颜色</param>
        /// <param name="brush_bg_active">激活主题色</param>
        /// <param name="brush_bg_activebg">激活背景色</param>
        /// <param name="brush_fore_active">激活字体色</param>
        /// <param name="datas">DATA</param>
        void PrintCalendar(Graphics g, Brush brush_fore, Brush brush_fore_disable, Brush brush_bg_disable, Brush brush_bg_active, Brush brush_bg_activebg, Brush brush_fore_active, List<Calendari> datas)
        {
            foreach (var it in datas)
            {
                using (var path = it.rect_read.RoundPath(Radius))
                {
                    bool hand = true;
                    if (it.t == 1 && SelDate != null)
                    {
                        if (SelDate.Length > 1)
                        {
                            if (SelDate[0] == SelDate[1])
                            {
                                if (SelDate[0].ToString("yyyy-MM-dd") == it.date_str)
                                {
                                    g.FillPath(brush_bg_active, path);
                                    g.DrawStr(it.v, Font, brush_fore_active, it.rect, s_f);
                                    hand = false;
                                }
                            }
                            else if (SelDate[0] <= it.date && SelDate[1] >= it.date)
                            {
                                //范围
                                if (SelDate[0].ToString("yyyy-MM-dd") == it.date_str)
                                {
                                    //前面
                                    g.FillRectangle(brush_bg_activebg, new RectangleF(it.rect_read.Right, it.rect_read.Y, it.rect.Width - it.rect_read.Width, it.rect_read.Height));
                                    using (var path_l = it.rect_read.RoundPath(Radius, true, false, false, true))
                                    {
                                        g.FillPath(brush_bg_active, path_l);
                                    }
                                    g.DrawStr(it.v, Font, brush_fore_active, it.rect, s_f);
                                }
                                else if (SelDate[1].ToString("yyyy-MM-dd") == it.date_str)
                                {
                                    //后面
                                    g.FillRectangle(brush_bg_activebg, new RectangleF(it.rect.X, it.rect_read.Y, it.rect_read.Width, it.rect_read.Height));
                                    using (var path_r = it.rect_read.RoundPath(Radius, false, true, true, false))
                                    {
                                        g.FillPath(brush_bg_active, path_r);
                                    }
                                    g.DrawStr(it.v, Font, brush_fore_active, it.rect, s_f);
                                }
                                else
                                {
                                    g.FillRectangle(brush_bg_activebg, new RectangleF(it.rect.X - 1F, it.rect_read.Y, it.rect.Width + 2F, it.rect_read.Height));
                                    g.DrawStr(it.v, Font, brush_fore, it.rect, s_f);
                                }
                                hand = false;
                            }
                        }
                        else if (SelDate[0].ToString("yyyy-MM-dd") == it.date_str)
                        {
                            g.FillPath(brush_bg_active, path);
                            g.DrawStr(it.v, Font, brush_fore_active, it.rect, s_f);
                            hand = false;
                        }
                    }
                    if (hand)
                    {
                        if ((oldTimeHover.HasValue && oldTime.HasValue) && it.date < oldTime.Value)
                        {
                            g.FillRectangle(brush_bg_disable, new RectangleF(it.rect.X, it.rect_read.Y, it.rect.Width, it.rect_read.Height));
                            g.DrawStr(it.v, Font, brush_fore_disable, it.rect, s_f);
                        }
                        else if ((oldTimeHover.HasValue && oldTime.HasValue) && it.t == 1 && (it.date == oldTime.Value || it.date == oldTimeHover.Value)) g.DrawStr(it.v, Font, brush_fore_active, it.rect, s_f);
                        else if (it.enable)
                        {
                            if (it.hover) g.FillPath(brush_bg_disable, path);
                            g.DrawStr(it.v, Font, it.t == 1 ? brush_fore : brush_fore_disable, it.rect, s_f);
                        }
                        else
                        {
                            g.FillRectangle(brush_bg_disable, new Rectangle(it.rect.X, it.rect_read.Y, it.rect.Width, it.rect_read.Height));
                            g.DrawStr(it.v, Font, brush_fore_disable, it.rect, s_f);
                        }
                    }
                }
            }

            if (badge_list.Count > 0)
            {
                using (var font = new Font(control.Font.FontFamily, control.Font.Size * control.BadgeSize))
                {
                    foreach (var it in datas)
                    {
                        if (badge_list.TryGetValue(it.date_str, out var find)) control.PaintBadge(find, font, it.rect, g);
                    }
                }
            }

            #region 渲染当天

            string nowstr = DateNow.ToString("yyyy-MM-dd");
            if (oldTimeHover.HasValue && oldTime.HasValue)
            {
                if (oldTime.Value.ToString("yyyy-MM-dd") == nowstr || oldTimeHover.Value.ToString("yyyy-MM-dd") == nowstr) return;
            }
            if (SelDate != null && SelDate.Length > 0)
            {
                if (SelDate.Length > 1)
                {
                    if (SelDate[1].ToString("yyyy-MM-dd") == nowstr) return;
                }
                else if (SelDate[0].ToString("yyyy-MM-dd") == nowstr) return;
            }
            foreach (var it in datas)
            {
                if (nowstr == it.date_str)
                {
                    using (var path = it.rect_read.RoundPath(Radius))
                    {
                        using (var pen_active = new Pen(Style.Db.Primary, Config.Dpi))
                        {
                            g.DrawPath(pen_active, path);
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #endregion

        Bitmap? shadow_temp = null;
        /// <summary>
        /// 绘制阴影
        /// </summary>
        /// <param name="g">GDI</param>
        /// <param name="rect">客户区域</param>
        void DrawShadow(Graphics g, Rectangle rect)
        {
            if (Config.ShadowEnabled)
            {
                if (shadow_temp == null)
                {
                    shadow_temp?.Dispose();
                    using (var path = new Rectangle(10, 10, rect.Width - 20, rect.Height - 20).RoundPath(Radius))
                    {
                        shadow_temp = path.PaintShadow(rect.Width, rect.Height);
                    }
                }
                g.DrawImage(shadow_temp, rect, 0.2F);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            hover_lefts?.Dispose(); hover_left?.Dispose(); hover_rights?.Dispose(); hover_right?.Dispose(); hover_year?.Dispose(); hover_month?.Dispose();
            hover_year_r?.Dispose(); hover_month_r?.Dispose();
            base.Dispose(disposing);
        }
    }
}