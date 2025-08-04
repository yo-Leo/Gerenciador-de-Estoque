// Classe de conexão com MySQL
using MySql.Data.MySqlClient;
using System;
using System.Globalization;

public static class Conexao
{
    public static MySqlConnection ObterConexao()
    {
        string conexaoString = "server=localhost;database=estoque;uid=root;pwd=;";
        MySqlConnection conexao = new MySqlConnection(conexaoString);
        conexao.Open();
        return conexao;
    }
}

// Classe utilitária de mensagens
public class Mensagem
{
    public void Invalida() => Console.WriteLine("\nOpção inválida. Tente novamente.\n");
    public void ErroDigitacao() => Console.WriteLine("\nErro na digitação. Use apenas números válidos.\n");
    public void Retorna() => Console.WriteLine("\nRetornando ao menu principal...\n");
}

// Classe responsável pelas operações de Produto
public class Produto
{
    public void CadastrarProd()
    {
        Console.Clear();
        Console.WriteLine("CADASTRO DE PRODUTO\n");

        Console.Write("Nome: ");
        string nome = Console.ReadLine();

        Console.Write("Preço: ");
        decimal preco = decimal.Parse(Console.ReadLine());

        Console.Write("Estoque: ");
        int estoque = int.Parse(Console.ReadLine());

        Console.Write("Categoria: ");
        string categoria = Console.ReadLine();

        Console.Write("Disponível (1 para sim, 0 para não): ");
        bool disponivel = Console.ReadLine() == "1";

        Console.Write("Descrição: ");
        string descricao = Console.ReadLine();

        using (var conn = Conexao.ObterConexao())
        {
            string query = "INSERT INTO Produto (nome_produto, preco, estoque, categoria, disponivel, descricao) VALUES (@nome, @preco, @estoque, @categoria, @disponivel, @descricao)";
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nome", nome);
                cmd.Parameters.AddWithValue("@preco", preco);
                cmd.Parameters.AddWithValue("@estoque", estoque);
                cmd.Parameters.AddWithValue("@categoria", categoria);
                cmd.Parameters.AddWithValue("@disponivel", disponivel);
                cmd.Parameters.AddWithValue("@descricao", descricao);

                cmd.ExecuteNonQuery();
                Console.WriteLine("\nProduto cadastrado com sucesso!");
            }
        }

        Console.WriteLine("\nPressione ENTER para continuar.");
        Console.ReadLine();
    }

    public static void ExibirProd(string comandosql)
    {
        Console.Clear();
        Console.WriteLine("==============================================\n");
        Console.WriteLine("             TABELA DE PRODUTOS               ");
        Console.WriteLine("\n==============================================\n");

        using (var conn = Conexao.ObterConexao())
        using (var cmd = new MySqlCommand(comandosql, conn))
        using (var reader = cmd.ExecuteReader())
        {
            if (!reader.HasRows)
            {
                Console.WriteLine("Nenhum produto localizado.");
            }
            else
            {
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["id_produto"]}");
                    Console.WriteLine($"NOME: {reader["nome_produto"]}");
                    Console.WriteLine($"PREÇO: R${Convert.ToDecimal(reader["preco"]).ToString("C", CultureInfo.CurrentCulture)}");
                    Console.WriteLine($"ESTOQUE: {reader["estoque"]}");
                    Console.WriteLine($"CATEGORIA: {reader["categoria"]}");
                    Console.WriteLine($"DISPONIVEL: {(Convert.ToBoolean(reader["disponivel"]) ? "Sim" : "Não")}");
                    Console.WriteLine($"\nDESCRIÇÃO:\n{reader["descricao"]}\n");
                    Console.WriteLine("================================================\n");
                }
            }
        }

        Console.WriteLine("Pressione ENTER para retornar ao menu.");
        Console.ReadLine();
    }

    public void RemoverProdutoPorNome(string nome)
    {
        using (var conn = Conexao.ObterConexao())
        {
            var query = "DELETE FROM Produto WHERE nome_produto = @nome";
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@nome", nome);
                int result = cmd.ExecuteNonQuery();
                Console.WriteLine(result > 0 ? "\nProduto removido com sucesso." : "\nProduto não encontrado.");
            }
        }

        Console.WriteLine("\nPressione ENTER para continuar.");
        Console.ReadLine();
    }

    public void AlterarQuantidade(string nome, int novaQuantidade)
    {
        using (var conn = Conexao.ObterConexao())
        {
            var query = "UPDATE Produto SET estoque = @estoque WHERE nome_produto = @nome";
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@estoque", novaQuantidade);
                cmd.Parameters.AddWithValue("@nome", nome);
                int result = cmd.ExecuteNonQuery();
                Console.WriteLine(result > 0 ? "\nQuantidade atualizada." : "\nProduto não encontrado.");
            }
        }

        Console.WriteLine("\nPressione ENTER para continuar.");
        Console.ReadLine();
    }
}

// Menu para consultas
public class MenuConsultaProduto
{
    Mensagem mensagem = new Mensagem();
    Produto produto = new Produto();

    public void ListarProdutos()
    {
        bool execute = true;

        while (execute)
        {
            Console.Clear();
            Console.WriteLine("==============================\n");
            Console.WriteLine("     PESQUISA DE PRODUTOS     ");
            Console.WriteLine("\n==============================\n");
            Console.WriteLine("[1] - Todos os produtos.");
            Console.WriteLine("[2] - Pesquisar por categoria.");
            Console.WriteLine("[3] - Pesquisar por nome.");
            Console.WriteLine("[4] - Sair.");
            Console.Write("\n>>> ");

            if (int.TryParse(Console.ReadLine(), out int escolha))
            {
                switch (escolha)
                {
                    case 1:
                        Produto.ExibirProd("SELECT * FROM Produto");
                        break;
                    case 2:
                        Console.WriteLine("\n[1] - Eletronico\n[2] - Roupas\n[3] - Alimento\n[4] - Livros\n[5] - Cosmeticos");
                        Console.Write("\n>>> ");
                        if (int.TryParse(Console.ReadLine(), out int cat))
                        {
                            string[] categorias = { "eletronico", "roupa", "alimento", "livro", "cosmetico" };
                            if (cat >= 1 && cat <= 5)
                                Produto.ExibirProd($"SELECT * FROM Produto WHERE categoria = '{categorias[cat - 1]}'");
                            else
                                mensagem.Invalida();
                        }
                        else mensagem.ErroDigitacao();
                        break;
                    case 3:
                        Console.Write("\nDigite o nome do produto: ");
                        string nome = Console.ReadLine();
                        Produto.ExibirProd($"SELECT * FROM Produto WHERE nome_produto LIKE '%{nome}%'");
                        break;
                    case 4:
                        mensagem.Retorna();
                        execute = false;
                        break;
                    default:
                        mensagem.Invalida();
                        break;
                }
            }
            else mensagem.ErroDigitacao();
        }
    }
}

// Menu principal do sistema
public class MenuPrincipal
{
    Mensagem mensagem = new Mensagem();
    MenuConsultaProduto consulta = new MenuConsultaProduto();
    Produto produto = new Produto();

    public bool Menu()
    {
        Console.Clear();
        Console.WriteLine("================================\n");
        Console.WriteLine("     GERENCIADOR DE ESTOQUE     ");
        Console.WriteLine("\n================================\n");
        Console.WriteLine("[1] - Consultar produtos.");
        Console.WriteLine("[2] - Cadastrar produtos.");
        Console.WriteLine("[3] - Remover produtos.");
        Console.WriteLine("[4] - Mudar quantidade.");
        Console.WriteLine("[5] - Sair.");
        Console.WriteLine("\n================================\n");
        Console.Write("\n>>> ");

        if (!int.TryParse(Console.ReadLine(), out int opcao))
        {
            mensagem.ErroDigitacao();
            return true;
        }

        switch (opcao)
        {
            case 1:
                consulta.ListarProdutos();
                break;
            case 2:
                produto.CadastrarProd();
                break;
            case 3:
                Console.Write("\nDigite o nome do produto a remover: ");
                produto.RemoverProdutoPorNome(Console.ReadLine());
                break;
            case 4:
                Console.Write("\nDigite o nome do produto para alterar quantidade: ");
                string nome = Console.ReadLine();
                Console.Write("Digite nova quantidade: ");
                if (int.TryParse(Console.ReadLine(), out int qtd))
                    produto.AlterarQuantidade(nome, qtd);
                else
                {
                    Console.WriteLine("\nQuantidade inválida.");
                    Console.ReadLine();
                }
                break;
            case 5:
                Console.Clear();
                Console.WriteLine("\nOBRIGADO POR UTILIZAR O SISTEMA!");
                Console.WriteLine("Pressione ENTER para sair.");
                Console.ReadLine();
                return false;
            default:
                mensagem.Invalida();
                break;
        }

        return true;
    }
}

// Classe principal do programa
class Program
{
    static void Main(string[] args)
    {
        MenuPrincipal menu = new MenuPrincipal();

        while (menu.Menu()) { }
    }
}
