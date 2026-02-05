// Свойства страницы




// Функция титульного листа

#let titlePage(
  doc: "",
  city: "Санкт-Петербург",
  university: "ГУАП",
  department: "Кафедра №43",
  teacherDegree: "доцент",
  teacherName: "Фоменкова А.А.",
  subject: "Основы Программирования",
  typeOfPaper: "Курсовая работа",
  nameOfPaper: "Разработка и внедрение программного средства для автоматизированного контроля сроков годности продуктов питания и непродовольственных товаров",
  group: "М411",
  studentName: "Колесников М.Ю.",
  studentSignature: "",
) = {
  // Шрифт и размер текста
  set text(font: "Times New Roman", size: 12pt)
  // Межстрочный интервал
  set par(spacing: 1.5em)
  // Отступы и размеры страницы
  set page(width: 210mm, height: 297mm, margin: (top: 20mm, bottom: 20mm, left: 30mm, right: 15mm))
  // Центрирование по умолчанию
  align(center)[

    //// Вёрстка страницы
    // Университет и кафедра
    #university

    #department]

  v(1fr)

  // Данные преподавателя и оценка
  align(left)[
    ОТЧЁТ
    #linebreak()
    ЗАЩИЩЁН С ОЦЕНКОЙ
    #linebreak()
    ПРЕПОДАВАТЕЛЬ
  ]
  grid(
    align: center,
    gutter: 4pt,
    columns: (1fr, 0.125fr, 1fr, 0.125fr, 1fr),
    // Линии
    grid.hline(y: 1, start: 0, end: 1),
    grid.hline(y: 1, start: 2, end: 3),
    grid.hline(y: 1, start: 4, end: 5),
    [#teacherDegree], [], [], [], [#teacherName],
    [должность, уч. степень, звание], [], [подпись, дата], [], [фамилия, инициалы], [],
  )

  v(1fr)

  // Название, тип работы, дисциплина
  align(center)[

    #upper(typeOfPaper)

    *#upper(nameOfPaper)*

    по курсу: *#subject*
  ]

  v(1fr)

  // Данные студента
  align(left)[РАБОТУ ВЫПОЛНИЛ]

  grid(
    align: center,
    gutter: 2pt,
    columns: (1fr, 0.125fr, 1fr, 0.125fr, 1fr),
    grid.hline(y: 1, start: 0, end: 1),
    grid.hline(y: 1, start: 2, end: 3),
    grid.hline(y: 1, start: 4, end: 5),
    [СТУДЕНТ ГР. № #group], [], [#studentSignature], [], [#studentName],
    [], [], [подпись, дата], [], [фамилия, инициалы],
  )

  v(1fr)

  // Город и год
  align(center)[
    #city
    #datetime.today().year()
  ]
  pagebreak()
  // Нумерация следующей страницы
  set page(numbering: "1")
  counter(page).update(1)
  doc
}



