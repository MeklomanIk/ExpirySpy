// gost.typ
// Базовые настройки страницы по ГОСТ Р 7.0.11–2011

// Формат страницы A4 и поля
#set page(
  width: 210mm,
  height: 297mm,
  margin: (
    left: 30mm,
    right: 10mm,
    top: 20mm,
    bottom: 20mm,
  ),
)

// Основной текст: Times New Roman, 14 пт, полуторный интервал
#set text(
  font: "Times New Roman",
  size: 14pt,
  spacing: 1.5em,
)

// Выравнивание по ширине и абзацный отступ 1.25 см
#set par(
  justify: true,
  first-line-indent: 1.25cm,
  spacing: 0pt,
)

// Заголовки разделов
#set heading(
  numbering: "1.",
  hanging-indent: 0pt,
)

// Стили для заголовков разных уровней
#show heading.where(level: 1): it => [
  #set text(weight: "bold")
  #set par(first-line-indent: 0pt)
  #it
]

#show heading.where(level: 2): it => [
  #set text(weight: "bold")
  #set par(first-line-indent: 0pt)
  #it
]

// Нумерация страниц по центру снизу (кроме титульного листа — его обычно выключают вручную)
#set page(numbering: "1", number-align: center)

// Подписи к рисункам
#let figure-caption(body) = [
  #set par(justify: true, first-line-indent: 1.25cm)
  Рисунок #counter(figure).display(): #body
]

// Подписи к таблицам
#let table-caption(body) = [
  #set par(justify: true, first-line-indent: 1.25cm)
  Таблица #counter(table).display() — #body
]

// Межабзацные интервалы для списков (чтобы не выглядело как студенческий ад)
#set list(
  spacing: 0.5em,
)

// Нумерация выражений
#set math.equation(
 numbering: "(1)"
)
