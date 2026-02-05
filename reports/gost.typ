

#let init(body) = {

  set text(
    font: "Times New Roman",
    size: 14pt,
    lang: "ru",
    region: "ru",
    hyphenate: true,
  )
  set heading(numbering: "1.1")

  set par(justify: true, leading: 1.2em)

  show table: set text(hyphenate: true)
  show table: set par(justify: false, leading: 0.3em, first-line-indent: 0em)

  set figure.caption(separator: [ --- ])

  show figure.where(kind: image): set figure(supplement: "Рисунок")
  show figure.where(kind: table): set figure(supplement: "Таблица")

  show figure.where(kind: table): set figure.caption(position: top)

  show figure.where(kind: table): it => {
    show figure.caption: set align(left)
    it
  }

  show figure: it =>{
    v(0.8em)
    it
    v(0.8em)

  }

  set list(marker: [---], body-indent: 0.7em, indent: 1.25cm)

  show list: set par(hanging-indent: -4.2em)
    
  set enum(numbering: "1.", body-indent: 0.7em, indent: 1.25cm)
  show enum: set par(hanging-indent: -4.2em)

    
  // Ссылка на изображения (без рисунок, просто число)
  show ref: it =>{
    let el = it.element

    if el != none and el.func() == figure {
      numbering(el.numbering, el.counter.at(el.location()).at(0))
    } else if el != none and el.func() == math.equation {
      // Override equation references.
      link(
        el.location(),
        numbering(el.numbering, ..counter(math.equation).at(el.location())),
      )
    } else {
      it
    }
  }


  // titlepage
  

  set page(
    margin: (top: 20mm, bottom: 20mm, left: 20mm, right: 10mm),
  )
    
  // toc
    set page(numbering: "1")
    counter(page).update(2)


 {
    if true {
      show outline: set block(below: 1.25cm / 2)
      show heading: it => {
        set text(size: 14pt)
        set align(center)
        upper(it)
      }
      outline(depth: 3, indent: 0.5cm)
      pagebreak()
    }
  }
  show heading: it => block(width: 100%)[
    #set text(14pt, weight: "bold", hyphenate: false)

    #if (it.numbering != none) {
      counter(heading).display()
    }
    #text(it.body)
  ]

  set par(first-line-indent: (amount: 1.25cm, all: true))
  show figure.where(kind: table): set block(breakable: true)
  set math.equation(numbering: "(1)")
  

  show raw: set text(10pt, font: "JetBrains Mono")

  body
}


#let ch(content) = {
  align(heading(content, numbering: none), center)
}


// 1. Глобальный счетчик для уникальных ID (избавляет от ошибки datetime)
#let table-id-counter = counter("table-unique-id")

#let long-table(
  columns: auto,
  // Текст сверху на новой странице
  cont-header: [Продолжение с предыдущей страницы], 
  // Текст снизу перед разрывом
  cont-footer: [Продолжение на следующей странице], 
  ..args
) = {
  // Увеличиваем счетчик таблиц
  table-id-counter.step()
  
  context {
    // Получаем уникальный ID и определяем страницу начала
    let id = table-id-counter.get().first()
    let end-label = label("tbl-end-" + str(id))
    let start-page = here().page()
    
    // --- Внедряем метку в последнюю ячейку данных ---
    // Это гарантирует, что метка не "улетит" на пустую страницу
    let pos-args = args.pos()
    let named-args = args.named()
    
    if pos-args.len() > 0 {
      let last-idx = pos-args.len() - 1
      let last-content = pos-args.at(last-idx)
      pos-args.at(last-idx) = [#last-content #end-label]
    }
    // --------------------------------------------------

    let col-count = if type(columns) == int { columns } else { columns.len() }

    table(
      columns: columns,
      ..named-args,
      
      // --- ХЕДЕР (ВЕРХНЯЯ ЧАСТЬ) ---
      table.header(
        repeat: true, // Обязательно повторяем хедер
        table.cell(colspan: col-count, stroke: none, align: left + bottom, inset: 0pt)[
          #context {
            // Если текущая страница больше стартовой -> показываем надпись
            if here().page() > start-page {
               [
                Продолжение таблицы #counter(figure.where(kind: table)).display()
               ]
               v(0.5em) // Небольшой отступ до данных
               
            }
          }
        ]
      ),

      // --- ТЕЛО ТАБЛИЦЫ ---
      ..pos-args,

      
    )
  }
}
