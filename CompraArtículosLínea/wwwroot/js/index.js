// Para el meu
const navMenu = document.getElementById('nav-menu'),
      navOpcion = document.getElementById('nav-opcion'),
      navCierre = document.getElementById('nav-cierre')

/* Mostrar menu */
if(navOpcion){
    navOpcion.addEventListener('click', () =>{
        navMenu.classList.add('mostrar-menu')
    })
}

/* Ocultar menu */
if(navCierre){
    navCierre.addEventListener('click', () =>{
        navMenu.classList.remove('mostrar-menu')
    })
}

// Remover menu movil 

// Para el swiper

let swiperImag = new Swiper('.inicio__swiper', {
  loop: false, // Desactivamos el loop para avanzar en orden
  spaceBetween: 64,
  grabCursor: true,
  centeredSlides: true,

  autoplay: {
      delay: 3000, // Tiempo en milisegundos (3 segundos)
      disableOnInteraction: false, // Continúa el autoplay incluso si el usuario interactúa
  },

  pagination: {
      el: '.swiper-pagination',
      type: 'fraction',
  },

  navigation: {
      nextEl: '.swiper-button-next',
      prevEl: '.swiper-button-prev',
  },
});

let swiperTitulos = new Swiper('.inicio__titulos', {
  loop: false, // Desactivamos el loop para avanzar en orden
  spaceBetween: 64,
  grabCursor: true,
  centeredSlides: true,

  autoplay: {
      delay: 3000, // Asegúrate de que el delay sea el mismo en ambas instancias
      disableOnInteraction: false,
  },
});

// Sincronizar los Swipers manualmente
swiperImag.controller.control = swiperTitulos;
swiperTitulos.controller.control = swiperImag;

// Garantizar la sincronización manual
swiperImag.on('slideChangeTransitionStart', () => {
  swiperTitulos.slideTo(swiperImag.activeIndex);
});
swiperTitulos.on('slideChangeTransitionStart', () => {
  swiperImag.slideTo(swiperTitulos.activeIndex);
});

// Reiniciar autoplay al final
swiperImag.on('reachEnd', () => {
  swiperImag.autoplay.stop();
  setTimeout(() => {
      swiperImag.slideTo(0); // Reinicia en la primera diapositiva
      swiperImag.autoplay.start();
  }, 3000); // Espera 3 segundos antes de reiniciar
});
swiperTitulos.on('reachEnd', () => {
  swiperTitulos.autoplay.stop();
  setTimeout(() => {
      swiperTitulos.slideTo(0); // Reinicia en la primera diapositiva
      swiperTitulos.autoplay.start();
  }, 3000); // Espera 3 segundos antes de reiniciar
});


//   Cambiar el bg header

const bgHeader = () =>{
    const header = document.getElementById('header')
    this.scrollY >= 50 ? header.classList.add('bg-header') 
                       : header.classList.remove('bg-header')
}
window.addEventListener('scroll', bgHeader)