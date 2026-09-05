(function ($) {
    'use strict';

    var page = 1;
    var pageSize = 8;

    function apiUrl(path) {
        var baseUrl = $('#api-base-url').val().replace(/\/$/, '');
        return baseUrl + path;
    }

    function showFeedback(selector, message, isError) {
        $(selector).text(message || '').toggleClass('error', !!isError);
    }

    function loadStudents() {
        showFeedback('#student-feedback', 'Carregando...', false);
        $.getJSON(apiUrl('/api/alunos'), {
            nome: $('#student-search').val(),
            incluirInativos: $('#include-inactive').is(':checked'),
            pagina: page,
            tamanho: pageSize
        }).done(function (data) {
            var rows = data.Itens || data.itens || [];
            $('#students-body').html(rows.length ? rows.map(function (student) {
                var active = student.Ativo !== undefined ? student.Ativo : student.ativo;
                var status = active ? 'Ativo' : 'Inativo';
                return '<tr><td class="student-id">' + (student.Id !== undefined ? student.Id : student.id) + '</td><td>' + escapeHtml(student.Nome || student.nome) + '</td><td>' + escapeHtml(student.Email || student.email) + '</td><td>' + formatDate(student.DataNascimento || student.dataNascimento) + '</td><td>' + escapeHtml(student.Turmas || student.turmas || 'Sem turma') + '</td><td><span class="status ' + (active ? '' : 'inactive') + '">' + status + '</span></td></tr>';
            }).join('') : '<tr><td colspan="6">Nenhum aluno encontrado.</td></tr>');
            var currentPage = data.Pagina || data.pagina || page;
            var totalPages = data.TotalPaginas || data.totalPaginas || 1;
            $('#page-label').text('Pagina ' + currentPage + ' de ' + totalPages);
            $('#previous-page').prop('disabled', page <= 1);
            $('#next-page').prop('disabled', page >= totalPages);
            $('#active-count').text(data.Total || data.total || 0);
            showFeedback('#student-feedback', '', false);
            $('#last-updated').text('Atualizado agora');
        }).fail(function (xhr) {
            showFeedback('#student-feedback', errorMessage(xhr), true);
        });
    }

    function loadReport() {
        $.getJSON(apiUrl('/api/relatorios/alunos-por-turma')).done(function (items) {
            var totalSpots = 0;
            $('#report-grid').html((items || []).map(function (item) {
                var availableSpots = item.VagasDisponiveis !== undefined ? item.VagasDisponiveis : item.vagasDisponiveis;
                var studentCount = item.TotalAlunos !== undefined ? item.TotalAlunos : item.totalAlunos;
                totalSpots += availableSpots || 0;
                return '<article class="report-card"><h4>' + escapeHtml(item.TurmaNome || item.turmaNome) + '</h4><p>' + escapeHtml(item.Periodo || item.periodo) + '</p><p><strong>' + studentCount + '</strong> alunos / ' + availableSpots + ' vagas livres</p></article>';
            }).join('') || '<p>Nenhuma turma encontrada.</p>');
            $('#class-count').text((items || []).length);
            $('#spot-count').text(totalSpots);
        }).fail(function () {
            $('#report-grid').html('<p class="feedback error">Nao foi possivel carregar o relatorio.</p>');
        });
    }

    function loadClasses() {
        $.getJSON(apiUrl('/api/turmas')).done(function (items) {
            $('#enrollment-class-id').html('<option value="">Selecione uma turma</option>' + (items || []).map(function (item) {
                var id = item.Id !== undefined ? item.Id : item.id;
                var name = item.Nome || item.nome;
                var availableSpots = item.VagasDisponiveis !== undefined ? item.VagasDisponiveis : item.vagasDisponiveis;
                return '<option value="' + id + '">' + escapeHtml(name) + ' - ' + availableSpots + ' vagas livres</option>';
            }).join(''));
        }).fail(function () {
            $('#enrollment-class-id').html('<option value="">Nao foi possivel carregar turmas</option>');
        });
    }

    function submitStudent(event) {
        event.preventDefault();
        showFeedback('#form-feedback', 'Salvando...', false);
        $.ajax({
            url: apiUrl('/api/alunos'),
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                nome: $('#student-name').val(),
                email: $('#student-email').val(),
                dataNascimento: $('#student-birth').val()
            })
        }).done(function () {
            $('#student-form')[0].reset();
            showFeedback('#form-feedback', 'Aluno cadastrado com sucesso.', false);
            page = 1;
            loadStudents();
        }).fail(function (xhr) {
            showFeedback('#form-feedback', errorMessage(xhr), true);
        });
    }

    function submitEnrollment(event) {
        event.preventDefault();
        showFeedback('#enrollment-feedback', 'Processando matricula...', false);
        $.ajax({
            url: apiUrl('/api/matriculas'),
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                alunoId: Number($('#enrollment-student-id').val()),
                turmaId: Number($('#enrollment-class-id').val())
            })
        }).done(function () {
            $('#enrollment-form')[0].reset();
            showFeedback('#enrollment-feedback', 'Matricula efetivada com sucesso.', false);
            loadClasses();
            loadReport();
        }).fail(function (xhr) {
            showFeedback('#enrollment-feedback', errorMessage(xhr), true);
        });
    }

    function formatDate(value) {
        if (!value) return '--';
        return value.substring(0, 10).split('-').reverse().join('/');
    }

    function escapeHtml(value) {
        return $('<div>').text(value || '').html();
    }

    function errorMessage(xhr) {
        return xhr.responseJSON?.message || xhr.responseJSON?.Message || 'Nao foi possivel conectar a API.';
    }

    $('#search-form').on('submit', function (event) { event.preventDefault(); page = 1; loadStudents(); });
    $('#include-inactive').on('change', function () { page = 1; loadStudents(); });
    $('#student-form').on('submit', submitStudent);
    $('#enrollment-form').on('submit', submitEnrollment);
    $('#previous-page').on('click', function () { if (page > 1) { page--; loadStudents(); } });
    $('#next-page').on('click', function () { page++; loadStudents(); });
    $('#refresh-all').on('click', function () { loadStudents(); loadReport(); });
    $('#refresh-report').on('click', loadReport);
    loadStudents();
    loadClasses();
    loadReport();
}(jQuery));