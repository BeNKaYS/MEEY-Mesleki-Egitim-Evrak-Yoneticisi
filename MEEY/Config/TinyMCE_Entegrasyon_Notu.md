# TinyMCE Entegrasyon Notu (Metin Editörü)

Tarih: 2026-02-28

## Alınan Karar
- Metin editörü için TinyMCE kullanılacak.
- Frontend yaklaşımı: React içinde `@tinymce/tinymce-react`.

## Referans React Bileşeni
```jsx
import React from 'react';
import { Editor } from '@tinymce/tinymce-react';

export default function App() {
  return (
    <Editor
      apiKey={process.env.REACT_APP_TINYMCE_API_KEY}
      init={{
        plugins: [
          'anchor', 'autolink', 'charmap', 'codesample', 'emoticons', 'link', 'lists', 'media', 'searchreplace', 'table', 'visualblocks', 'wordcount',
          'checklist', 'mediaembed', 'casechange', 'formatpainter', 'pageembed', 'a11ychecker', 'tinymcespellchecker', 'permanentpen', 'powerpaste', 'advtable', 'advcode', 'advtemplate', 'ai', 'uploadcare', 'mentions', 'tinycomments', 'tableofcontents', 'footnotes', 'mergetags', 'autocorrect', 'typography', 'inlinecss', 'markdown', 'importword', 'exportword', 'exportpdf'
        ],
        toolbar: 'undo redo | blocks fontfamily fontsize | bold italic underline strikethrough | link media table mergetags | addcomment showcomments | spellcheckdialog a11ycheck typography uploadcare | align lineheight | checklist numlist bullist indent outdent | emoticons charmap | removeformat',
        tinycomments_mode: 'embedded',
        tinycomments_author: 'Author name',
        mergetags_list: [
          { value: 'First.Name', title: 'First Name' },
          { value: 'Email', title: 'Email' },
        ],
        ai_request: (request, respondWith) => respondWith.string(() => Promise.reject('See docs to implement AI Assistant')),
        uploadcare_public_key: '3aed63bdf07f17d913ed',
      }}
      initialValue="Welcome to TinyMCE!"
    />
  );
}
```

## Anahtar Konumu
- TinyMCE API anahtarı yerel dosyada saklanır:
  - `MEEY/Config/local/tinymce.apikey.txt`
- Bu klasör `.gitignore` içine eklendi, repoya dahil edilmez.

## Sonraki Adım
- React editör host'u oluşturulacak.
- WPF `WebView2` ile editör ekranına gömülecek.
